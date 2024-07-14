using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	internal class ItemHandler : MonoBehaviour
	{
		private static ItemHandler instance;
		private readonly List<ItemData.Item> itemsInQueue = new();
		private readonly List<StatusType> statusBuffs = new();
		private readonly List<StatusType> statusDebuffs = new();
		private List<ItemData.Item> itemData;
		private Dictionary<string, int> dungeonKeyCounts;
		private FadeEffectData fadeData;
		private Entity player;
		private EntityStatusable playerStatusable;
		private SaverOwner mainSaver;
		private GameObject beeSwarmSpawner;
		private SoundClip heartSound;
		private Stopwatch stopwatch;
		private bool hasStoredRefs;
		private bool needsToSave;

		public static ItemHandler Instance { get { return instance; } }
		public bool HasInitialized { get; private set; }
		public bool HasSaved { get; private set; }

		public enum ItemType
		{
			None, // Default
			Bees,
			Buff,
			Card,
			CaveScroll,
			Crayon,
			Debuff,
			EFCS,
			Heart,
			Key,
			Keyring,
			Melee,
			Outfit,
			PortalWorldScroll,
			Upgrade
		}

		private void Awake()
		{
			instance = this;

			// Parse item JSON
			if (!ModCore.Utility.TryParseJson($@"{PluginInfo.PLUGIN_NAME}\Data\itemData.json", out ItemData? data))
			{
				Plugin.Log.LogError($"ItemHandler failed to deserialize item data JSON and will do nothing!");
				return;
			}

			itemData = data?.Items;
			mainSaver = ModCore.Plugin.MainSaver;
			dungeonKeyCounts = new()
			{
				{ "PillowFort", 2 },
				{ "SandCastle", 2 },
				{ "ArtExhibit", 4 },
				{ "TrashCave", 4 },
				{ "FloodedBasement", 5 },
				{ "PotassiumMines", 5 },
				{ "BoilingGrave", 5 },
				{ "GrandLibrary", 8 },
				{ "SunkenLabyrinth", 3 },
				{ "MachineFortress", 5 },
				{ "DarkHypostyle", 5 },
				{ "TombOfSimulacrum", 10 },
				{ "DreamDynamite", 3 },
				{ "DreamFireChain", 4 },
				{ "DreamIce", 4 },
				{ "DreamAll", 4 }
			};
			fadeData = ItemRandomizer.Instance.FadeData;

			Events.OnSceneLoaded += OnSceneLoaded;

			OverlayFader.StartFade(fadeData, true, delegate ()
			{
				stopwatch = Stopwatch.StartNew();
				ModCore.Utility.LoadScene("Deep7");
			}, Vector3.zero);
		}

		public int GetItemCount(ItemData.Item item, out bool isLevelItem)
		{
			isLevelItem = false;

			if (player == null || mainSaver == null)
				return 0;

			if (item.Type == ItemType.Key)
			{
				string dungeonName = item.ItemName.Substring(0, item.ItemName.IndexOf("Key") - 1).Replace(" ", "");
				IDataSaver keySaver = mainSaver.GetSaver($"/local/levels/{dungeonName}/player/vars");
				return keySaver.LoadInt("localKeys");
			}

			List<string> levelItems = new() { "chain", "tome", "amulet", "headband", "tracker" };
			List<string> countItems = new() { "shards", "raft", "evilKeys" };

			isLevelItem = item.Type == ItemType.Upgrade || levelItems.Contains(item.Flag);

			if (isLevelItem || countItems.Contains(item.Flag))
				return player.GetStateVariable(item.Flag);

			return 0;
		}

		public ItemData.Item GetItemData(string itemName)
		{
			if (itemData == null)
				return null;

			return itemData.Find(item => item.ItemName == itemName);
		}

		public ItemData.Item GetItemData(int offset)
		{
			if (itemData == null)
				return null;

			return itemData.Find(item => item.Offset == offset);
		}

		public void GiveItem(ItemData.Item item)
		{
			itemsInQueue.Add(item);
		}

		private void Update()
		{
			if (player == null)
				return;

			if (itemsInQueue.Count == 1)
				needsToSave = true;

			if (itemsInQueue.Count > 0)
			{
				DoGiveItem(itemsInQueue[0]);
				itemsInQueue.RemoveAt(0);
			}

			if (needsToSave)
			{
				mainSaver?.SaveLocal(false, false);
				HasSaved = true;
				needsToSave = false;
			}
		}

		public void DoGiveItem(ItemData.Item item)
		{
			switch (item.Type)
			{
				case ItemType.Bees:
					Plugin.StartRoutine(SpawnBees());
					break;
				case ItemType.Buff:
					ApplyRandomStatus(statusBuffs);
					break;
				case ItemType.Card:
					AddCard(item.Flag);
					break;
				case ItemType.CaveScroll:
					AddScroll(true);
					break;
				case ItemType.Crayon:
					AddCrayon();
					break;
				case ItemType.Debuff:
					ApplyRandomStatus(statusDebuffs);
					break;
				case ItemType.EFCS:
					AddEFCS();
					break;
				case ItemType.Heart:
					AddHeart();
					break;
				case ItemType.Key:
					AddKeys(item.ItemName, false);
					break;
				case ItemType.Keyring:
					AddKeys(item.ItemName, true);
					break;
				case ItemType.Outfit:
					AddOutfit(item.Flag);
					break;
				case ItemType.PortalWorldScroll:
					AddScroll(false);
					break;
				case ItemType.Upgrade:
					AddUpgrade(item.Flag);
					break;
				default:
					if (!string.IsNullOrEmpty(item.Flag))
						IncrementItem(item);
					break;
			}
		}

		public void OnPlayerSpawned(Entity player)
		{
			this.player = player;
			playerStatusable = player.GetEntityComponent<EntityStatusable>();

			if (hasStoredRefs)
				HasInitialized = true;
		}

		private void AddCard(string saveFlag)
		{
			mainSaver.GetSaver("/local/cards").SaveInt(saveFlag, 1);
		}

		private void AddCrayon()
		{
			Killable killable = player.GetEntityComponent<Killable>();
			killable.MaxHp += 1;
			killable.CurrentHp = killable.MaxHp;
		}

		private void AddEFCS()
		{
			mainSaver.GetSaver("/local/levels/TombOfSimulacrum/N").SaveInt("PuzzleDoor_green-100--22", 1);
			mainSaver.GetSaver("/local/levels/TombOfSimulacrum/S").SaveInt("PuzzleDoor_green-64--25", 1);
			mainSaver.GetSaver("/local/levels/Deep17/B").SaveInt("PuzzleGate-23--5", 1);
		}

		private void AddHeart()
		{
			player.GetEntityComponent<Killable>().CurrentHp += 20;

			if (heartSound == null)
				heartSound = Resources.FindObjectsOfTypeAll<DummyQuickEffect>().FirstOrDefault(x => x.gameObject.name == "PickupHeartEffect")._sound;

			if (heartSound != null)
				SoundPlayer.Instance.PlayPositionedSound(heartSound, player.transform.position);
		}

		private void AddKeys(string itemName, bool giveAll)
		{
			string dungeonName = itemName.Substring(0, itemName.IndexOf("Key") - 1).Replace(" ", "");
			IDataSaver keySaver = mainSaver.GetSaver($"/local/levels/{dungeonName}/player/vars");

			if (giveAll)
			{
				if (dungeonKeyCounts.TryGetValue(dungeonName, out int maxKeyCount))
				{
					keySaver.SaveInt("localKeys", maxKeyCount);
				}

				return;
			}

			int currentKeyCount = keySaver.LoadInt("localKeys");
			keySaver.SaveInt("localKeys", currentKeyCount + 1);
			return;
		}

		private void AddOutfit(string saveFlag)
		{
			int outfitNum = int.Parse((Regex.Match(saveFlag, @"\d+").Value));
			mainSaver.GetSaver("/local/world").SaveInt(saveFlag, 1);
			player.SetStateVariable(saveFlag.Replace(outfitNum.ToString(), ""), outfitNum);
		}

		// TODO
		private void AddScroll(bool cave)
		{
			LogNotImplementedMessage(cave ? "Cave Scroll" : "Portal World Scroll");
		}

		private void AddUpgrade(string upgradeFlag)
		{
			// Remove word "Upgrade" from saveFlag
			string itemFlag = upgradeFlag.Substring(0, upgradeFlag.Length - 7);
			int upgradeAmount = player.GetStateVariable(upgradeFlag);
			// If upgrade has already been obtained, increment by 1, otherwise start at level 2
			int newUpgradeAmount = upgradeAmount == 0 ? 2 : upgradeAmount + 1;
			// Set new upgrade level
			player.SetStateVariable(upgradeFlag, newUpgradeAmount);

			// If upgrade is obtained after item, set item level directly
			if (player.GetStateVariable(itemFlag) > 0)
				player.SetStateVariable(itemFlag, newUpgradeAmount);
		}

		private void ApplyRandomStatus(List<StatusType> statuses)
		{
			StatusType randomStatus = statuses[Random.Range(0, statuses.Count)];
			playerStatusable.AddStatus(randomStatus);
		}

		private void IncrementItem(ItemData.Item item)
		{
			int currentLevel = player.GetStateVariable(item.Flag);

			// If at max level already, don't do anything
			if (item.Max > 0 && currentLevel >= item.Max)
				return;

			int upgradeLevel = player.GetStateVariable(item.Flag + "Upgrade");
			// If no upgrade level, increment by 1, otherwise use upgrade level
			int newLevel = upgradeLevel == 0 ? currentLevel + 1 : upgradeLevel;
			player.SetStateVariable(item.Flag, newLevel);
		}

		private IEnumerator SpawnBees()
		{
			yield return new WaitForEndOfFrame();

			if (beeSwarmSpawner == null)
				yield return null;

			beeSwarmSpawner.transform.position = player.transform.position;
			beeSwarmSpawner.GetComponent<SpawnEntityEventObserver>().OnFire(false);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (hasStoredRefs)
			{
				Events.OnSceneLoaded -= OnSceneLoaded;
				stopwatch.Stop();
				Plugin.Log.LogInfo($"Took {stopwatch.ElapsedMilliseconds}ms to store references");
				return;
			}

			StatusType[] statuses = Resources.FindObjectsOfTypeAll<StatusType>();

			// Former Colossus
			if (scene.name == "Deep7")
			{
				StoreStatus(statuses, true, "DireHit");
				StoreStatus(statuses, true, "Hearty");
				StoreStatus(statuses, true, "Tough");
				StoreStatus(statuses, true, "Mighty");
				StoreStatus(statuses, false, "Cold");
				StoreStatus(statuses, false, "Fragile");
				StoreStatus(statuses, false, "Weak");
				ModCore.Utility.LoadScene("MachineFortress");
			}
			else if (scene.name == "MachineFortress")
			{
				StoreStatus(statuses, false, "Fear");
				beeSwarmSpawner = ModCore.Utility.FindNestedChild("LevelRoot", "Dungeon_ChestBees").gameObject;
				beeSwarmSpawner.transform.parent = null;
				DontDestroyOnLoad(beeSwarmSpawner);
				hasStoredRefs = true;
				fadeData._fadeOutTime = 0;
				IDataSaver startSaver = mainSaver.GetSaver("/local/start");
				string savedScene = startSaver.LoadData("level");
				string sceneToLoad = string.IsNullOrEmpty(savedScene) ? "Intro" : savedScene;
				SceneDoor.StartLoad(sceneToLoad, startSaver.LoadData("door"), fadeData);
			}
		}

		private void StoreStatus(StatusType[] loadedStatuses, bool isBuff, string name)
		{
			StatusType status = loadedStatuses.FirstOrDefault(status => status.name.EndsWith(name));

			if (status != null)
			{
				status._overrides = [];

				if (isBuff)
					statusBuffs.Add(status);
				else
					statusDebuffs.Add(status);

				DontDestroyOnLoad(status);
			}
		}

		private void LogNotImplementedMessage(string itemName)
		{
			Plugin.Log.LogWarning($"Obtained {itemName}, but this is not implemented yet, so nothing happens!");
		}

		public struct ItemData
		{
			public List<Item> Items { get; set; }

			public class Item
			{
				public string ItemName { get; set; }
				public string IconName { get; set; } = "APProgression"; // Default AP icon
				public int Offset { get; set; }
				public string Flag { get; set; }
				public ItemType Type { get; set; }
				public int Max { get; set; }
			}
		}
	}
}