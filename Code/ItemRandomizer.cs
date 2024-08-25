using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ArchipelagoRandomizer.MessageBoxHandler;

namespace ArchipelagoRandomizer
{
	public class ItemRandomizer : MonoBehaviour
	{
		public static event OnItemReceievedFunc OnItemReceived;
		private static ItemRandomizer instance;
		private static List<LocationData.Location> locations;
		private static FadeEffectData fadeData;

		private MessageBoxHandler itemMessageHandler;
		private ItemHandler itemHandler;
		private DeathLinkHandler deathLinkHandler;
		private HintHandler hintHandler;
		private GoalHandler goalHandler;
		private SceneLoadEvents sceneEventHandler;
		private RoomLoadEvents roomEventHandler;
		private Entity player;
		private SaverOwner mainSaver;
		private RandomizerSettings settings;
		private PlayerActionModifier playerActionModifier;
		private LootMenuHandler lootMenuHandler;
		private bool rollOpensChests;
		private bool hasSyncedItemsWithServer;

		public static ItemRandomizer Instance { get { return instance; } }
		public static bool IsActive { get; private set; }
		public static List<LocationData.Location> Locations { get { return locations; } }
		public FadeEffectData FadeData { get { return fadeData; } }

		public void OnFileStart(bool newFile, APFileData apFileData)
		{
			settings = new RandomizerSettings();
			mainSaver = ModCore.Plugin.MainSaver;
			rollOpensChests = settings.RollOpensChests;

			if (newFile)
				new NewFileEvents(settings, mainSaver, apFileData);

			PreloadObjects();

			IsActive = true;
			Plugin.Log.LogInfo("ItemRandomizer is enabled!");
		}

		public void RollToOpenChest(List<CollisionDetector.CollisionData> collisions)
		{
			if (!rollOpensChests)
				return;

			foreach (CollisionDetector.CollisionData collision in collisions)
			{
				RandomizedObject objData = collision.gameObject.GetComponentInParent<RandomizedObject>();
				bool isChest = objData != null && objData.ObjType == RandomizedObject.ObjectType.Chest;

				if (!isChest)
					continue;

				Transform crystal = collision.transform.parent.Find("crystal");
				bool isLockedByCrystal = crystal != null && crystal.gameObject.activeSelf;

				if (isLockedByCrystal)
					continue;

				DummyAction action = crystal == null ?
					collision.transform.GetComponentInParent<DummyAction>() :
					collision.transform.parent.GetChild(0).GetComponent<DummyAction>();

				if (action == null || action.hasFired)
					continue;

				action.Fire(false);
			}
		}

		public void LocationChecked(string saveFlag, string sceneName)
		{
			if (string.IsNullOrEmpty(saveFlag))
				return;

			LocationData.Location location = locations.Find(x => (string.IsNullOrEmpty(x.SceneName) || x.SceneName == sceneName) && x.Flag == saveFlag);

			if (location == null)
			{
				Plugin.Log.LogError($"No location with save flag {saveFlag} in {sceneName} was found in JSON data, so location will not be marked on Archipelago server!");
				return;
			}

			APHandler.Instance.LocationChecked(location.Offset);
		}

		public ItemHandler.ItemData.Item GetItemForLocation(string scene, string saveFlag, out Archipelago.MultiClient.Net.Models.ScoutedItemInfo scoutedItemInfo)
		{
			LocationData.Location location = locations.Find(x => x.SceneName == scene && x.Flag == saveFlag);
			scoutedItemInfo = APHandler.Instance.GetScoutedItemInfo(location);

			if (scoutedItemInfo == null)
				return null;

			return itemHandler.GetItemData(scoutedItemInfo.ItemDisplayName);
		}

		public IEnumerator ItemSent(string itemName, string playerName)
		{
			// Wait for player to spawn
			while (player == null)
				yield return null;

			MessageData messageData = new()
			{
				Item = itemHandler.GetItemData(itemName),
				ItemName = itemName,
				PlayerName = playerName,
				MessageType = MessageType.Sent
			};
			itemMessageHandler.ShowMessageBox(messageData);
		}

		public IEnumerator ItemReceived(int offset, string itemName, string sentFromPlayer)
		{
			ItemHandler.ItemData.Item item = itemHandler.GetItemData(offset);

			// Do nothing if null item
			if (item == null)
				yield return null;

			// Wait for player to spawn
			while (player == null)
				yield return null;

			// Assign item
			itemHandler.GiveItem(item);

			yield return new WaitForEndOfFrame();

			// Send item get message
			MessageData messageData = new()
			{
				Item = item,
				ItemName = itemName,
				PlayerName = sentFromPlayer,
				MessageType = sentFromPlayer == APHandler.Instance.CurrentPlayer.Name ?
					MessageType.ReceivedFromSelf :
					MessageType.ReceivedFromSomeone
			};
			itemMessageHandler.ShowMessageBox(messageData);

			OnItemReceived?.Invoke(item, sentFromPlayer);
		}

		public IEnumerator OnDisconnected()
		{
			yield return new WaitForEndOfFrame();
			mainSaver.SaveAll();
			SceneDoor.StartLoad("MainMenu", "", fadeData);
		}

		private void Awake()
		{
			instance = this;

			// Parse data JSON
			if (locations == null)
			{
				string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "locationData.json");

				if (!ModCore.Utility.TryParseJson(path, out LocationData? data))
				{
					Plugin.Log.LogError("ItemRandomizer JSON data has failed to load! The randomizer will not start!");
					Destroy(this);
					return;
				}

				locations = data?.Locations;
			}

			// Setup fade data
			if (fadeData == null)
			{
				fadeData = new()
				{
					_targetColor = Color.black,
					_fadeOutTime = 0.5f,
					_fadeInTime = 1.25f,
					_faderName = "ScreenCircleWipe",
					_useScreenPos = true
				};
			}

			DontDestroyOnLoad(this);
		}

		private void Start()
		{
			Events.OnChangeScreen += (string toScene, object args) =>
			{
				if (toScene == "itemRoot")
				{
					if (lootMenuHandler == null)
						lootMenuHandler = GameObject.Find("OverlayCamera").transform.Find("PauseOverlay_anchor/PauseOverlay/Pause/ItemScreen").gameObject.AddComponent<LootMenuHandler>();
				}
			};
		}

		private void OnDisable()
		{
			IsActive = false;
			sceneEventHandler.DoDisable();
			roomEventHandler.DoDisable();
			sceneEventHandler = null;
			roomEventHandler = null;

			Events.OnPlayerSpawn -= OnPlayerSpawn;

			// If disconnected, show message
			if (!APHandler.Instance.IsConnected)
			{
				MessageData messageData = new()
				{
					Message = "Oh no! You were disconnected from the server!"
				};
				itemMessageHandler.ShowMessageBox(messageData);
			}

			APHandler.Instance.Disconnect();

			Plugin.Log.LogInfo("ItemRandomizer is disabled!");
		}

		private void PreloadObjects()
		{
			Preloader preloader = new();

			// TODO: Store: Forbidden Key, Dynamite
			preloader.AddObjectToPreloadList("MachineFortress", () =>
			{
				return [
					GameObject.Find("LevelRoot").transform.Find("O/Doodads/Dungeon_ChestBees").gameObject,
					GameObject.Find("LevelRoot").transform.Find("G/Logic/SecretPortal").gameObject
				];
			});
			// Maze of Steel
			// TODO: Store: Headband
			preloader.AddObjectToPreloadList("Deep11", () =>
			{
				return Resources.FindObjectsOfTypeAll<StatusType>();
			});
			/*
			// TODO: Store outfits from changing tent, shards, lockpicks, crayons
			preloader.AddObjectToPreloadList("CandyCoastCaves", () =>
			{
				return [null];
			});
			// TODO: Store: Melees, Raft Piece, Dungeon Key
			preloader.AddObjectToPreloadList("TrashCave", () =>
			{
				return [null];
			});
			// Pepperpain Mountain
			// TODO: Store: Eruption
			preloader.AddObjectToPreloadList("VitaminHills3", () =>
			{
				return [null];
			});
			// Autumn Climb
			// TODO: Store: Tracker
			preloader.AddObjectToPreloadList("Deep1", () =>
			{
				return [null];
			});
			// Painful Plain
			// TODO: Store: Tome
			preloader.AddObjectToPreloadList("Deep3", () =>
			{
				return [null];
			});
			// Brutal Oasis
			// TODO: Store: Amulet
			preloader.AddObjectToPreloadList("Deep6", () =>
			{
				return [null];
			});
			// Ocean Castle
			// TODO: Store: Force Wand
			preloader.AddObjectToPreloadList("Deep9", () =>
			{
				return [null];
			});
			// Northern End
			// TODO: Store: Ice Ring
			preloader.AddObjectToPreloadList("Deep14", () =>
			{
				return [null];
			});
			*/
			// Moon Garden
			// TODO: Store: Chain
			preloader.AddObjectToPreloadList("Deep15", () =>
			{
				SpawnItemEventObserver observer = GameObject.Find("LevelRoot").transform.Find("A/Doodads/Dungeon_Chest").GetComponent<SpawnItemEventObserver>();
				GameObject chain = GameObject.Instantiate(FreestandingReplacer.GetGameObjectFromSelector(observer));
				Destroy(chain.GetComponent<VarUpdatingItem>());
				chain.name = "MC Chain";
				FreestandingReplacer.AddModelPreview("Progressive Chain", chain);
				return [chain];
			});
			/*
			// Bad Dream
			// TODO: Store: Card
			preloader.AddObjectToPreloadList("Deep26", () =>
			{
				return [null];
			});
			*/

			preloader.StartPreload(PreloadDone);
		}

		private void PreloadDone()
		{
			itemHandler = gameObject.AddComponent<ItemHandler>();
			itemMessageHandler = MessageBoxHandler.Instance;
			deathLinkHandler = Plugin.Instance.APFileData.Deathlink ? gameObject.AddComponent<DeathLinkHandler>() : null;
			hintHandler = gameObject.AddComponent<HintHandler>();
			goalHandler = gameObject.AddComponent<GoalHandler>();
			sceneEventHandler = new SceneLoadEvents(settings);
			roomEventHandler = new RoomLoadEvents(settings);
			playerActionModifier = new();

			Events.OnPlayerSpawn += OnPlayerSpawn;
		}

		private void OnPlayerSpawn(Entity player, GameObject camera, PlayerController controller)
		{
			this.player = player;

			if (!hasSyncedItemsWithServer && !Preloader.IsPreloading)
			{
				APHandler.Instance.SyncItemsWithServer();
				hasSyncedItemsWithServer = true;
			}
		}

		public struct LocationData
		{
			public List<Location> Locations { get; set; }

			public class Location
			{
				[JsonProperty("location")]
				public string LocationName { get; set; }
				public int Offset { get; set; }
				[JsonProperty("scene")]
				public string SceneName { get; set; }
				public string Flag { get; set; }
			}
		}

		public class APFileData
		{
			public string Server { get; set; }
			public int Port { get; set; }
			public string SlotName { get; set; }
			public string Password { get; set; }
			public bool Deathlink { get; set; }
			public bool AutoEquipOutfits { get; set; }
			public bool StackStatuses { get; set; }
			public bool ChestAppearanceMatchesContents { get; set; }
		}

		public delegate void OnItemReceievedFunc(ItemHandler.ItemData.Item item, string sentFromPlayerName);
	}
}