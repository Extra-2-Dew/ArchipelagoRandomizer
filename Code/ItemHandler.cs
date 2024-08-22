using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	public class ItemHandler : MonoBehaviour
	{
		private static ItemHandler instance;
		private static List<ItemData.Item> itemData;
		private static Dictionary<string, int> dungeonKeyCounts;
		private static bool hasInitialized;

		private readonly List<StatusType> statusBuffs = new();
		private readonly List<StatusType> statusDebuffs = new();
		private IDataSaver itemsObtainedSaver;
		private Entity player;
		private EntityStatusable playerStatusable;
		private SaverOwner mainSaver;
		private SpawnEntityEventObserver beeSwarmSpawner;
		private SoundClip heartSound;

		public static ItemHandler Instance { get { return instance; } }

		public enum ItemTypes
		{
			None,
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
			Shard,
			Upgrade
		}

		[System.Flags]
		public enum ItemFlags
		{
			None = 0,
			Macguffin = 1,
			Major = 2,
			Minor = 4,
			Junk = 8
		}

		public int GetItemCount(ItemData.Item item, out bool isLevelItem)
		{
			isLevelItem = false;
			Entity player = ModCore.Utility.GetPlayer();

			if (player == null || mainSaver == null)
				return 0;

			if (item.Type == ItemTypes.Key)
			{
				string dungeonName = item.ItemName.Substring(0, item.ItemName.IndexOf("Key") - 1).Replace(" ", "");

				// Handle dungeon names with different names than what is in item name
				switch (dungeonName)
				{
					case "TombofSimulacrum":
						dungeonName = "TombOfSimulacrum";
						break;
					case "Syncope":
						dungeonName = "DreamDynamite";
						break;
					case "Antigram":
						dungeonName = "DreamFireChain";
						break;
					case "BottomlessTower":
						dungeonName = "DreamIce";
						break;
					case "Quietus":
						dungeonName = "DreamAll";
						break;
				}

				IDataSaver keySaver = mainSaver.GetSaver($"/local/levels/{dungeonName}/player/vars");
				int keyCount = keySaver.LoadInt("localKeys");

				if (keyCount < 2)
					return 0;

				return keyCount;
			}

			List<string> levelItems = new() { "chain", "tome", "amulet", "headband", "tracker" };
			List<string> countItems = new() { "shards", "raft", "evilKeys" };

			isLevelItem = item.Type == ItemTypes.Upgrade || levelItems.Contains(item.SaveFlag);

			if (isLevelItem || countItems.Contains(item.SaveFlag))
				return player.GetStateVariable(item.SaveFlag);

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
			StartCoroutine(DoGiveItem(item));
		}

		private IEnumerator DoGiveItem(ItemData.Item item)
		{
			yield return new WaitForEndOfFrame();
			player = ModCore.Utility.GetPlayer();

			switch (item.Type)
			{
				case ItemTypes.Bees:
					Plugin.StartRoutine(SpawnBees());
					break;
				case ItemTypes.Buff:
					ApplyRandomStatus(false);
					break;
				case ItemTypes.Card:
					AddCard(item.SaveFlag);
					break;
				case ItemTypes.CaveScroll:
					AddScroll(true);
					break;
				case ItemTypes.Crayon:
					AddCrayon();
					break;
				case ItemTypes.Debuff:
					ApplyRandomStatus(true);
					break;
				case ItemTypes.EFCS:
					AddEFCS();
					break;
				case ItemTypes.Heart:
					AddHeart();
					break;
				case ItemTypes.Key:
					AddKeys(item.ItemName, false);
					break;
				case ItemTypes.Keyring:
					AddKeys(item.ItemName, true);
					break;
				case ItemTypes.Outfit:
					AddOutfit(item.SaveFlag);
					break;
				case ItemTypes.PortalWorldScroll:
					AddScroll(false);
					break;
				case ItemTypes.Upgrade:
					AddUpgrade(item);
					break;
				default:
					if (!string.IsNullOrEmpty(item.SaveFlag))
						IncrementItem(item);
					break;
			}

			// Saves obtained flag (used for AP sync when connecting)
			itemsObtainedSaver.SaveInt("count", itemsObtainedSaver.LoadInt("count") + 1);
			itemsObtainedSaver.SaveInt(item.ItemName, itemsObtainedSaver.LoadInt(item.ItemName) + 1);

			// Yes, this saves after every item assignment instead of batchng it
			// This is because Ludo's save system sucks and can't be read when I need it to
			// If I batch save, so I don't care
			mainSaver.SaveLocal(false, false);
		}

		private void Awake()
		{
			instance = this;

			if (!hasInitialized)
			{
				// Parse item JSON
				string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "itemData.json");

				if (!ModCore.Utility.TryParseJson(path, out ItemData? data))
				{
					Plugin.Log.LogError($"ItemHandler failed to deserialize item data JSON and will do nothing!");
					Destroy(this);
					return;
				}

				itemData = data?.Items;
				dungeonKeyCounts = new()
				{
					{ "PillowFort", 2 },
					{ "SandCastle", 2 },
					{ "ArtExhibit", 4 },
					{ "TrashCave", 4 },
					{ "FloodedBasement", 5 },
					{ "PotassiumMine", 5 },
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
				hasInitialized = true;
			}

			mainSaver = ModCore.Plugin.MainSaver;
			itemsObtainedSaver = itemsObtainedSaver = mainSaver.GetSaver("/local/archipelago/itemsObtained");
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
			mainSaver.GetSaver("/local/levels/TombOfSimulacrum/AC").SaveInt("PuzzleGate-48--54", 1);
			mainSaver.GetSaver("/local/levels/Deep17/B").SaveInt("PuzzleGate-23--5", 1);
			player.SetStateVariable("fakeEFCS", 1);
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
			string flagName = "localKeys";
			int keysToGive = 0;

			// Handle dungeon names with different names than what is in item name
			switch (dungeonName)
			{
				case "TombofSimulacrum":
					dungeonName = "TombOfSimulacrum";
					break;
				case "Syncope":
					dungeonName = "DreamDynamite";
					break;
				case "Antigram":
					dungeonName = "DreamFireChain";
					break;
				case "BottomlessTower":
					dungeonName = "DreamIce";
					break;
				case "Quietus":
					dungeonName = "DreamAll";
					break;
			}

			if (giveAll && !dungeonKeyCounts.TryGetValue(dungeonName, out keysToGive))
				return;

			// If not in the dungeon the key is for
			if (dungeonName != SceneManager.GetActiveScene().name)
			{
				IDataSaver keySaver = mainSaver.GetSaver($"/local/levels/{dungeonName}/player/vars");
				keysToGive = giveAll ? keysToGive : keySaver.LoadInt(flagName) + 1;
				keySaver.SaveInt(flagName, keysToGive);
				return;
			}

			// If in the dungeon the key is for
			keysToGive = giveAll ? keysToGive : player.GetStateVariable(flagName) + 1;
			player.SetStateVariable(flagName, keysToGive);
		}

		private void AddOutfit(string saveFlag)
		{
			int outfitNum = int.Parse((Regex.Match(saveFlag, @"\d+").Value));
			mainSaver.GetSaver("/local/world").SaveInt(saveFlag, 1);

			if (Plugin.Instance.APFileData.AutoEquipOutfits)
				player.SetStateVariable(saveFlag.Replace(outfitNum.ToString(), ""), outfitNum);
		}

		// TODO
		private void AddScroll(bool cave)
		{
			LogNotImplementedMessage(cave ? "Cave Scroll" : "Portal World Scroll");
		}

		private void AddUpgrade(ItemData.Item item)
		{
			// Remove word "Upgrade" from saveFlag
			string upgradeFlag = item.SaveFlag;
			string itemFlag = upgradeFlag.Substring(0, upgradeFlag.Length - 7);
			int upgradeAmount = player.GetStateVariable(upgradeFlag);

			if (item.Max > 0 && upgradeAmount >= item.Max)
				return;

			// If upgrade has already been obtained, increment by 1, otherwise start at level 2
			int newUpgradeAmount = upgradeAmount == 0 ? 2 : upgradeAmount + 1;
			// Set new upgrade level
			player.SetStateVariable(upgradeFlag, newUpgradeAmount);

			// If upgrade is obtained after item, set item level directly
			if (player.GetStateVariable(itemFlag) > 0)
				player.SetStateVariable(itemFlag, newUpgradeAmount);
		}

		private void ApplyRandomStatus(bool debuff)
		{
			if (statusDebuffs.Count == 0 || statusBuffs.Count == 0)
				SortStatuses();

			StatusType randomStatus = debuff ?
				statusDebuffs[Random.Range(0, statusDebuffs.Count)] :
				statusBuffs[Random.Range(0, statusBuffs.Count)];

			if (playerStatusable == null)
				playerStatusable = player.GetEntityComponent<EntityStatusable>();

			playerStatusable.AddStatus(randomStatus);
		}

		private void IncrementItem(ItemData.Item item)
		{
			int currentLevel = player.GetStateVariable(item.SaveFlag);

			// If at max level already, don't do anything
			if (item.Max > 0 && currentLevel >= item.Max)
				return;

			int upgradeLevel = player.GetStateVariable(item.SaveFlag + "Upgrade");
			// If no upgrade level, increment by 1, otherwise use upgrade level
			int newLevel = upgradeLevel == 0 ? currentLevel + 1 : upgradeLevel;
			player.SetStateVariable(item.SaveFlag, newLevel);
		}

		private IEnumerator SpawnBees()
		{
			yield return new WaitForEndOfFrame();

			if (beeSwarmSpawner == null)
			{
				GameObject beeSwarmSpawnerObj = Preloader.GetPreloadedObject<GameObject>("Dungeon_ChestBees");
				beeSwarmSpawnerObj.transform.position = player.transform.position;
				beeSwarmSpawner = beeSwarmSpawnerObj.GetComponent<SpawnEntityEventObserver>();
			}

			beeSwarmSpawner.OnFire(false);
		}

		private void SortStatuses()
		{
			List<string> buffs = new() { "DireHit", "Hearty", "Tough", "Mighty" };
			List<string> debuffs = new() { "Cold", "Fragile", "Weak" };

			foreach (Object obj in Preloader.GetAllPreloadedObjects())
			{
				if (obj is StatusType status)
				{
					string statName = status.name.Substring(5);

					if (debuffs.Contains(statName))
					{
						statusDebuffs.Add(status);
						continue;
					}

					if (buffs.Contains(statName))
						statusBuffs.Add(status);
				}
			}

			Plugin.Log.LogInfo("Stored statuses!");
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
				public string SaveFlag { get; set; }
				public ItemTypes Type { get; set; }
				public ItemFlags Flag { get; set; }
				public int Max { get; set; }
			}
		}
	}
}