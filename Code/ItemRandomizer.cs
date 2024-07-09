using SmallJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class ItemRandomizer
	{
		public static ItemRandomizer Instance { get { return instance; } }
		public bool HasInitialized { get; private set; }
		public bool IsActive { get; private set; }

		private static ItemRandomizer instance;
		private readonly List<LocationData> locationData;
		private readonly List<ItemData> itemData;
		private readonly Dictionary<string, int> dungeonKeyCounts = new()
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
			{ "DreamAll", 4 },
		};
		private ItemMessageHandler itemMessageHandler;
		private SoundClip heartSound;

		public ItemRandomizer()
		{
			instance = this;

			// Parse data JSON
			locationData = ParseLocationJson();
			itemData = ParseItemJson();
			HasInitialized = locationData != null && itemData != null;

			if (!HasInitialized)
			{
				Plugin.Log.LogError("ItemRandomizer JSON data has failed to load! The randomizer will not start!");
				return;
			}

			itemMessageHandler = new();

			if (Plugin.TestingLocally)
			{
				string server = "localhost:38281";
				//string server = "archipelago.gg:58159";
				string slot = "ChrisID2";
				if (APHandler.Instance.TryCreateSession(server, slot, "", out string message))
					Plugin.Log.LogInfo($"Successfully connected to Archipelago server '{server}' as '{slot}'!");
				else
					Plugin.Log.LogInfo($"Failed to connect to Archipelago server '{server}'!");
			}
		}

		public void SetupNewFile(bool newFile)
		{
			if (!HasInitialized)
				return;

			if (newFile)
			{
				SaverOwner saver = ModCore.Plugin.MainSaver;
				saver.LocalStorage.GetLocalSaver("settings").SaveInt("hideCutscenes", 1);
				saver.SaveAll();

				UnlockWarpGarden();
				ObtainedTracker3();
			}

			IsActive = true;
			Plugin.Log.LogInfo("Item randomizer is now active!");
		}

		private void UnlockWarpGarden()
		{
			List<string> warpLetters = new() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
			SaverOwner saver = ModCore.Plugin.MainSaver;
			saver.WorldStorage.SaveInt("WorldWarpE", 1);

			for (int i = 0; i < warpLetters.Count; i++)
			{
				string warp = "WorldWarp" + warpLetters[i];
				saver.WorldStorage.SaveInt(warp, 1);
				saver.GetSaver("/local/markers").SaveData(warp, warp);
			}

			saver.SaveAll();
		}

		private void ObtainedTracker3()
		{
			Dictionary<string, string[]> scenesAndRooms = new()
			{
				{ "FluffyFields", new string[] { "A", "B", "C" } },
				{ "CandyCoast", new string[] { "A", "B", "C" } },
				{ "FancyRuins", new string[] { "C", "B", "A" } },
				{ "FancyRuins2", new string[] { "A" } },
				{ "StarWoods", new string[] { "A", "B", "C" } },
				{ "StarWoods2", new string[] { "A" } },
				{ "SlipperySlope", new string[] { "A", "B" } },
				{ "VitaminHills", new string[] { "A", "B", "C" } },
				{ "VitaminHills2", new string[] { "A" } },
				{ "VitaminHills3", new string[] { "A" } },
				{ "FrozenCourt", new string[] { "A" } },
				{ "LonelyRoad", new string[] { "A", "B", "C", "D", "E" } },
				{ "LonelyRoad2", new string[] { "A" } },
				{ "Deep2", new string[] { "A", "B", "C", "D" } },
				{ "Deep3", new string[] { "B", "A" } },
				{ "Deep4", new string[] { "A", "B" } },
				{ "Deep5", new string[] { "A", "B", "C", "D", "E", "F", "G" } },
				{ "Deep6", new string[] { "B", "A" } },
				{ "Deep7", new string[] { "A", "B" } },
				{ "Deep8", new string[] { "A", "B", "C", "D" } },
				{ "Deep9", new string[] { "B", "A" } },
				{ "Deep10", new string[] { "B", "A" } },
				{ "Deep11", new string[] { "A", "B", "C", "D", "E", "F" } },
				{ "Deep12", new string[] { "A", "B", "C" } },
				{ "Deep13", new string[] { "A", "B", "C", "D", "E" } },
				{ "Deep14", new string[] { "A", "B", "C", "D", "E", "F" } },
				{ "Deep15", new string[] { "B", "A" } },
				{ "Deep17", new string[] { "A", "B" } },
				{ "Deep19s", new string[] { "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" } },
				{ "Deep20", new string[] { "A", "B" } },
				{ "Deep22", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" } },
				{ "PillowFort", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" } },
				{ "SandCastle", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" } },
				{ "ArtExhibit", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S" } },
				{ "TrashCave", new string[] { "J", "A", "B", "C", "D", "E", "F", "G", "H", "I", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" } },
				{ "FloodedBasement", new string[] { "M", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "N", "O", "P", "Q", "R", "S", "T", "U" } },
				{ "PotassiumMine", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U" } },
				{ "BoilingGrave", new string[] { "V", "A", "AA", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "W", "X", "Y", "Z" } },
				{ "GrandLibrary", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE" } },
				{ "GrandLibrary2", new string[] { "BA", "BB", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "DA", "DB", "DC", "DD", "DE", "DF", "DG", "DH", "EA" } },
				{ "SunkenLabyrinth", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U" } },
				{ "MachineFortress", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R" } },
				{ "DarkHypostyle", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W" } },
				{ "TombOfSimulacrum", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ" } },
				{ "DreamForce", new string[] { "B", "C", "E", "I", "J", "K", "L", "M", "Y", "W", "X", "Z", "AA", "AE", "AF", "AG", "AH", "AD" } },
				{ "DreamDynamite", new string[] { "A", "B", "C", "D", "E", "F", "I", "K", "L", "N", "R", "V", "W", "X", "Y", "Z", "AB", "AF", "AG", "AH", "AI", "AL", "AM", "AN", "AO", "AS", "AT", "AU" } },
				{ "DreamFireChain", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R" } },
				{ "DreamIce", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "AA", "Z", "AB", "AC", "AD", "AE" } },
				{ "DreamAll", new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD" } },
			};
			SaverOwner saver = ModCore.Plugin.MainSaver;

			// Mark all rooms as visited
			foreach (KeyValuePair<string, string[]> sceneAndRoom in scenesAndRooms)
			{
				for (int i = 0; i < sceneAndRoom.Value.Length; i++)
				{
					saver.GetSaver($"/local/levels/{sceneAndRoom.Key}/player/seenrooms").SaveInt(sceneAndRoom.Value[i], 1);
				}
			}

			saver.SaveAll();
		}

		public void LocationChecked(string saveFlag)
		{
			if (string.IsNullOrEmpty(saveFlag))
				return;

			LocationData location = locationData.Find(x => x.Flag == saveFlag);

			if (location == null)
			{
				Plugin.Log.LogError($"No location with save flag {saveFlag} was found in JSON data, so location will not be marked on Archipelago server!");
				return;
			}

			APHandler.Instance.LocationChecked(location.Offset);
		}

		public void ItemSent(string itemName, string playerName)
		{
			ItemData item = itemData.Find(x => x.ItemName == itemName);
			Plugin.StartRoutine(itemMessageHandler.ShowMessageBox(ItemMessageHandler.MessageType.Sent, item, itemName, playerName));
		}

		public void ItemReceived(int offset, string itemName, string sentFromPlayer)
		{
			ItemData item = itemData.Find(x => x.Offset == offset);

			if (item == null)
				return;

			Plugin.StartRoutine(GiveItem(item));
			ItemMessageHandler.MessageType messageType = sentFromPlayer == APHandler.Instance.CurrentPlayer.Name ?
				ItemMessageHandler.MessageType.ReceivedFromSelf : ItemMessageHandler.MessageType.ReceivedFromSomeone;
			Plugin.StartRoutine(itemMessageHandler.ShowMessageBox(messageType, item, itemName, sentFromPlayer));
		}

		private IEnumerator GiveItem(ItemData item)
		{
			yield return new WaitForEndOfFrame();
			SaverOwner saver = ModCore.Plugin.MainSaver;
			Entity player = EntityTag.GetEntityByName("PlayerEnt");
			Dictionary<string, int> flagsToSet = new();

			switch (item.Type)
			{
				case ItemData.ItemType.Heart:
					// Heals 20 HP (5 hearts)
					player.GetEntityComponent<Killable>().CurrentHp += 20;

					if (heartSound == null)
						heartSound = Resources.FindObjectsOfTypeAll<DummyQuickEffect>().FirstOrDefault(x => x.gameObject.name == "PickupHeartEffect")._sound;

					SoundPlayer.instance.PlayPositionedSound(heartSound, player.transform.position);
					break;
				case ItemData.ItemType.Crayon:
					// Increase max HP by 1 and heals
					Killable killable = player.GetEntityComponent<Killable>();
					killable.MaxHp += 1;
					killable.CurrentHp = killable.MaxHp;
					break;
				case ItemData.ItemType.Key:
					// Increment key count for scene
					string dungeonName = item.ItemName.Substring(0, item.ItemName.IndexOf("Key") - 1).Replace(" ", "");
					IDataSaver keySaver = saver.GetSaver($"/local/levels/{dungeonName}/player/vars");
					int currentKeyCount = keySaver.LoadInt("localKeys");
					keySaver.SaveInt("localKeys", currentKeyCount + 1);

					break;
				case ItemData.ItemType.Keyring:
					// Set max key count for scene
					string dungeonName2 = item.ItemName.Substring(0, item.ItemName.IndexOf("Key") - 1).Replace(" ", "");

					if (dungeonKeyCounts.TryGetValue(dungeonName2, out int maxKeyCount))
					{
						IDataSaver keySaver2 = saver.GetSaver($"/local/levels/{dungeonName2}/player/vars");
						keySaver2.SaveInt("localKeys", maxKeyCount);
					}

					break;
				case ItemData.ItemType.Outfit:
					// Sets world flag for outfit in changing tent + equips outfit
					int outfitNum = int.Parse(Regex.Match(item.Flag, @"\d+").Value);
					flagsToSet.Add(item.Flag.Replace(outfitNum.ToString(), ""), outfitNum);
					saver.GetSaver("/local/world").SaveInt(item.Flag, 1);
					break;
				case ItemData.ItemType.CaveScroll:
					Plugin.Log.LogWarning("Obtained Cave Scroll, but this is not implemented yet, so nothing happens!");
					break;
				case ItemData.ItemType.PortalWorldScroll:
					Plugin.Log.LogWarning("Obtained Portal World Scroll, but this is not implemented yet, so nothing happens!");
					break;
				case ItemData.ItemType.EFCS:
					// Sets the flags for the couple EFCS gates/doors that are EFCS only
					Plugin.Log.LogWarning("Obtained Fake EFCS, but this is not implemented yet, so nothing happens!");
					break;
				case ItemData.ItemType.Card:
					// Sets card flag
					saver.GetSaver("/local/cards").SaveInt(item.Flag, 1);
					break;
				case ItemData.ItemType.Upgrade:
					string itemFlag = item.Flag.Substring(0, item.Flag.Length - 7);
					int upgradeAmount = player.GetStateVariable(item.Flag);
					int newUpgradeAmount = upgradeAmount == 0 ? 2 : upgradeAmount + 1;
					flagsToSet.Add(item.Flag, newUpgradeAmount);

					// If upgrade is obtained after item
					if (player.GetStateVariable(itemFlag) > 0)
						flagsToSet.Add(itemFlag, newUpgradeAmount);
					break;
				default:
					// Increment level/count by 1
					if (string.IsNullOrEmpty(item.Flag))
						break;

					int upgradeLevel = player.GetStateVariable(item.Flag + "Upgrade");
					int newAmount = upgradeLevel == 0 ? player.GetStateVariable(item.Flag) + 1 : upgradeLevel;
					flagsToSet.Add(item.Flag, newAmount);
					break;
			}

			foreach (KeyValuePair<string, int> flag in flagsToSet)
			{
				// Don't set flag if value is already at max
				if (item.Max > 0 && flag.Value > item.Max)
					continue;

				player.SetStateVariable(flag.Key, flag.Value);
				Plugin.Log.LogInfo($"Set flag {flag.Key} to {flag.Value}!");
			}

			saver.SaveLocal();
		}

		private List<LocationData> ParseLocationJson()
		{
			if (!ModCore.Utility.TryParseJson(PluginInfo.PLUGIN_NAME, "Data", "locationData.json", out JsonObject rootObj))
				return null;

			List<LocationData> locations = new();

			foreach (JsonObject locationObj in rootObj.GetArray("locations").objects.Cast<JsonObject>())
			{
				string locationName = locationObj.GetString("location");
				int offset = locationObj.GetInt("offset");
				string flag = locationObj.GetString("flag");

				locations.Add(new LocationData(locationName, offset, flag));
			}

			return locations;
		}

		private List<ItemData> ParseItemJson()
		{
			if (!ModCore.Utility.TryParseJson(PluginInfo.PLUGIN_NAME, "Data", "itemData.json", out JsonObject rootObj))
				return null;

			List<ItemData> items = new();

			foreach (JsonObject itemObj in rootObj.GetArray("items").objects.Cast<JsonObject>())
			{
				string itemName = itemObj.GetString("itemName");
				string iconName = itemObj.GetString("iconName");
				int offset = itemObj.GetInt("offset");
				string flag = itemObj.GetString("flag");
				string typeStr = itemObj.GetString("type");
				int max = itemObj.GetInt("max");

				ItemData.ItemType type = !String.IsNullOrEmpty(typeStr) ? (ItemData.ItemType)Enum.Parse(typeof(ItemData.ItemType), typeStr) : ItemData.ItemType.None;

				items.Add(new ItemData(itemName, iconName, offset, flag, type, max));
			}

			return items;
		}

		public class LocationData
		{
			public string Location { get; }
			public int Offset { get; }
			public string Flag { get; }

			public LocationData(string location, int offset, string flag)
			{
				Location = location;
				Offset = offset;
				Flag = flag;
			}
		}

		public class ItemData
		{
			public string ItemName { get; }
			public string IconName { get; } = "APProgression"; // Default AP icon
			public int Offset { get; }
			public string Flag { get; }
			public ItemType Type { get; }
			public int Max { get; }

			public enum ItemType
			{
				None, // Default
				Card,
				CaveScroll,
				Crayon,
				EFCS,
				Heart,
				Key,
				Keyring,
				Melee,
				Outfit,
				PortalWorldScroll,
				Upgrade
			}

			public ItemData(string itemName, string iconName, int offset, string flag, ItemType type, int max)
			{
				ItemName = itemName;
				IconName = iconName;
				Offset = offset;
				Flag = flag;
				Type = type;
				Max = max;
			}
		}
	}
}