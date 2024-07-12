using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	internal class ItemHandler
	{
		private readonly List<ItemData.Item> itemData;
		private readonly List<StatusType> statusBuffs = new();
		private readonly List<StatusType> statusDebuffs = new();
		private readonly Dictionary<string, int> dungeonKeyCounts;
		private readonly FadeEffectData fadeData;
		private GameObject beeSwarmSpawner;
		private SoundClip heartSound;
		private SaverOwner mainSaver;
		private Entity player;
		private Stopwatch stopwatch;
		private bool hasStoredRefs;

		public bool HasInitialized { get; private set; }

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

		public ItemHandler()
		{
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
			fadeData = new()
			{
				_targetColor = Color.black,
				_fadeOutTime = 0.5f,
				_fadeInTime = 1.25f,
				_faderName = "ScreenCircleWipe",
				_useScreenPos = true
			};

			Events.OnSceneLoaded += OnSceneLoaded;

			OverlayFader.StartFade(fadeData, true, delegate ()
			{
				stopwatch = Stopwatch.StartNew();
				ModCore.Utility.LoadScene("Deep7");
			}, Vector3.zero);
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

		public IEnumerator GiveItem(ItemData.Item item)
		{
			yield return new WaitForEndOfFrame();

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

			mainSaver?.SaveLocal();
		}

		public void OnPlayerSpawned(Entity player)
		{
			this.player = player;

			if (hasStoredRefs)
				HasInitialized = true;

			if (statusBuffs.Count > 0 || statusDebuffs.Count > 0)
				return;

			EntityStatusable statusable = player.GetEntityComponent<EntityStatusable>();

			foreach (StatusType status in statusable._saveable)
			{
				// These are not to be used currently
				if (status.name.EndsWith("Courage") || status.name.EndsWith("Fortune") || status.name.EndsWith("Curse"))
					continue;

				if (status.name.EndsWith("Fragile") || status.name.EndsWith("Weak"))
					statusDebuffs.Add(status);
				else
					statusBuffs.Add(status);

				Object.DontDestroyOnLoad(status);
			}
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

		private void AddUpgrade(string saveFlag)
		{
			// Remove word "Upgrade" from saveFlag
			string flagSubstr = saveFlag.Substring(0, saveFlag.Length - 7);
			int upgradeAmount = player.GetStateVariable(saveFlag);
			// If upgrade has already been obtained, increment by 1, otherwise start at level 2
			int newUpgradeAmount = upgradeAmount == 0 ? 2 : upgradeAmount + 1;
			// Set new upgrade level
			player.SetStateVariable(flagSubstr, newUpgradeAmount);

			// If upgrade is obtained after item, set item level directly
			if (player.GetStateVariable(flagSubstr) > 0)
				player.SetStateVariable(flagSubstr, newUpgradeAmount);
		}

		private void ApplyRandomStatus(List<StatusType> statuses)
		{
			StatusType randomStatus = statuses[Random.Range(0, statuses.Count - 1)];
			player.GetEntityComponent<EntityStatusable>().AddStatus(randomStatus);
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

			SpawnEntityEventObserver spawner = beeSwarmSpawner.GetComponent<SpawnEntityEventObserver>();
			spawner._entity.DoSpawn(player.transform.position, Vector3.zero, false);
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

			// Former Colossus
			if (scene.name == "Deep7")
			{
				StoreStatus("Cold");
				ModCore.Utility.LoadScene("MachineFortress");
			}
			else if (scene.name == "MachineFortress")
			{
				StoreStatus("Fear");
				beeSwarmSpawner = ModCore.Utility.FindNestedChild("LevelRoot", "Dungeon_ChestBees").gameObject;
				beeSwarmSpawner.transform.parent = null;
				Object.DontDestroyOnLoad(beeSwarmSpawner);
				hasStoredRefs = true;
				fadeData._fadeOutTime = 0;
				IDataSaver startSaver = mainSaver.GetSaver("/local/start");
				string savedScene = startSaver.LoadData("level");
				string sceneToLoad = string.IsNullOrEmpty(savedScene) ? "Intro" : savedScene;
				SceneDoor.StartLoad(sceneToLoad, startSaver.LoadData("door"), fadeData);
			}
		}

		private void StoreStatus(string name)
		{
			StatusType status = Resources.FindObjectsOfTypeAll<StatusType>().FirstOrDefault(status => status.name.EndsWith(name));

			if (status != null)
			{
				statusDebuffs.Add(status);
				Object.DontDestroyOnLoad(status);
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