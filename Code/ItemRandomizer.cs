using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class ItemRandomizer
	{
		public static ItemRandomizer Instance { get { return instance; } }
		public bool IsActive { get; private set; }

		private static ItemRandomizer instance;
		private readonly List<LocationData.Location> locations;
		private readonly ItemMessageHandler itemMessageHandler;
		private ItemHandler itemHandler;

		public ItemRandomizer()
		{
			// Parse data JSON
			if (!ModCore.Utility.TryParseJson(@$"{PluginInfo.PLUGIN_NAME}\Data\locationData.json", out LocationData? data))
			{
				Plugin.Log.LogError("ItemRandomizer JSON data has failed to load! The randomizer will not start!");
				return;
			}

			instance = this;
			locations = data?.Locations;
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
			if (locations == null)
				return;

			if (newFile)
			{
				SaverOwner saver = ModCore.Plugin.MainSaver;
				saver.LocalStorage.GetLocalSaver("settings").SaveInt("hideCutscenes", 1);
				saver.SaveAll();

				UnlockWarpGarden();
				ObtainedTracker3();
			}

			itemHandler = new();
			IsActive = true;
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

		public void RollToOpenChest(List<CollisionDetector.CollisionData> collisions)
		{
			foreach (CollisionDetector.CollisionData collision in collisions)
			{
				bool isChest = collision.gameObject.GetComponentInParent<SpawnItemEventObserver>() != null;

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

		public void LocationChecked(string saveFlag)
		{
			if (string.IsNullOrEmpty(saveFlag))
				return;

			LocationData.Location location = locations.Find(x => x.Flag == saveFlag);

			if (location == null)
			{
				Plugin.Log.LogError($"No location with save flag {saveFlag} was found in JSON data, so location will not be marked on Archipelago server!");
				return;
			}

			APHandler.Instance.LocationChecked(location.Offset);
		}

		public void ItemSent(string itemName, string playerName)
		{
			ItemHandler.ItemData.Item item = itemHandler.GetItemData(itemName);
			Plugin.StartRoutine(itemMessageHandler.ShowMessageBox(ItemMessageHandler.MessageType.Sent, item, itemName, playerName));
		}

		public void ItemReceived(int offset, string itemName, string sentFromPlayer)
		{
			ItemHandler.ItemData.Item item = itemHandler.GetItemData(offset);

			if (item == null)
				return;

			Plugin.StartRoutine(itemHandler.GiveItem(item));
			ItemMessageHandler.MessageType messageType = sentFromPlayer == APHandler.Instance.CurrentPlayer.Name ?
				ItemMessageHandler.MessageType.ReceivedFromSelf : ItemMessageHandler.MessageType.ReceivedFromSomeone;
			Plugin.StartRoutine(itemMessageHandler.ShowMessageBox(messageType, item, itemName, sentFromPlayer));
		}

		public struct LocationData
		{
			public List<Location> Locations { get; set; }

			public class Location
			{
				[JsonProperty("location")]
				public string LocationName { get; set; }
				public int Offset { get; set; }
				public string Flag { get; set; }
			}
		}
	}
}