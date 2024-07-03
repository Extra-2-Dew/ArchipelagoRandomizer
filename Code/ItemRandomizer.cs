using SmallJson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class ItemRandomizer
	{
		private static ItemRandomizer instance;
		private readonly List<LocationData> locationData;
		private readonly List<ItemData> itemData;

		public static ItemRandomizer Instance { get { return instance; } }
		public bool HasInitialized { get; private set; }
		public bool IsActive { get; private set; }

		public ItemRandomizer(bool newFile)
		{
			instance = this;

			// Parse data JSON
			locationData = ParseLocationJson();
			itemData = ParseItemJson();
			HasInitialized = locationData != null && itemData != null;

			// TEMP
			string server = "localhost:38281";
			string slot = "WyrmID";
			if (APHandler.Instance.TryCreateSession(server, slot, "", out string message))
				Plugin.Log.LogInfo($"Successfully connected to Archipelago server '{server}' as '{slot}'!");
			else
				Plugin.Log.LogInfo($"Failed to connect to Archipelago server '{server}'!");

			if (newFile && HasInitialized)
				SetupNewFile();

			IsActive = true;
		}

		internal void SetupNewFile()
		{
			SaverOwner saver = ModCore.Plugin.MainSaver;
			saver.LocalStorage.GetLocalSaver("settings").SaveInt("hideCutscenes", 1);
			saver.SaveAll();
			UnlockWarpGarden();
			ObtainedTracker3();
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

		public void LocationChecked(ItemDataForRandomizer itemData)
		{
			if (itemData.Entity == null)
				itemData.Entity = EntityTag.GetEntityByName("PlayerEnt");

			if (string.IsNullOrEmpty(itemData.SaveFlag) || itemData.Entity == null || itemData.Item == null)
				return;

			LocationData location = locationData.Find(x => x.Flag == itemData.SaveFlag);

			if (location == null)
			{
				Plugin.Log.LogError($"No location with save flag {itemData.SaveFlag} was found in JSON data, so location will not be marked on Archipelago server!");
				return;
			}

			APHandler.Instance.LocationChecked(location.Offset);
		}

		public void ItemSent(string itemName, string playerName)
		{
			ItemData item = itemData.Find(x => x.Item == itemName);

			if (item == null)
				return;

			ShowItemSentHud(item, playerName);
		}

		public void ItemReceived(int offset, string sentFromPlayer)
		{
			ItemData item = itemData.Find(x => x.Offset == offset);

			if (item == null)
				return;

			ShowItemGetHud(item, sentFromPlayer);
		}

		private void ShowItemSentHud(ItemData itemData, string playerName)
		{
			string message = $"You found {itemData.Item} for {playerName}!";
			string picPath = $"Items/ItemIcon_{itemData.PicName}";

			Plugin.StartRoutine(ShowHud(message, picPath));
		}

		private void ShowItemGetHud(ItemData itemData, string sentFromPlayer)
		{
			string message = sentFromPlayer == APHandler.Instance.CurrentPlayer.Name ?
				$"You found your own {itemData.Item}!" :
				$"{sentFromPlayer} found your {itemData.Item}!";
			string picPath = $"Items/ItemIcon_{itemData.PicName}";

			Plugin.StartRoutine(ShowHud(message, picPath));
		}

		private IEnumerator ShowHud(string message, string picPath)
		{
			EntityHUD hud = EntityHUD.GetCurrentHUD();
			ItemMessageBox messageBox = EntityHUD.GetCurrentHUD().currentMsgBox;

			// Hides the message box if it's still shown from a prior one
			if (messageBox != null && messageBox.IsActive)
				messageBox.Hide(true);

			// Gets the current message box window
			messageBox = OverlayWindow.GetPooledWindow(hud._data.GetItemBox);

			// Shows the message box
			if (messageBox._tweener != null)
				messageBox._tweener.Show(true);
			else
				messageBox.gameObject.SetActive(true);

			// Waits for end of frame to avoid random issues with setting icon
			yield return new WaitForEndOfFrame();

			// Update item icon
			Texture2D texture = Resources.Load(picPath) as Texture2D;

			if (messageBox.texture != texture)
				Resources.UnloadAsset(messageBox.texture);

			messageBox.texture = texture;
			messageBox.mat.mainTexture = texture;

			// Waits for end of frame to avoid random issues with setting text
			yield return new WaitForEndOfFrame();

			// Updates the text
			messageBox._text.StringText = new StringHolder.OutString(message);

			// Update sizing
			Vector2 scaledTextSize = messageBox._text.ScaledTextSize;
			Vector3 vector = messageBox._text.transform.localPosition - messageBox.backOrigin;
			scaledTextSize.y += Mathf.Abs(vector.y) + messageBox._border;
			scaledTextSize.y = Mathf.Max(messageBox.minSize.y, scaledTextSize.y);
			scaledTextSize.x = messageBox._background.ScaledSize.x;
			messageBox._background.ScaledSize = scaledTextSize;

			// Sets timer
			messageBox.timer = messageBox._showTime;
			messageBox.countdown = messageBox._showTime > 0;
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

			List<ItemData> items2 = new();

			foreach (JsonObject itemObj in rootObj.GetArray("items").objects.Cast<JsonObject>())
			{
				string itemName = itemObj.GetString("item");
				int offset = itemObj.GetInt("offset");
				string flag = itemObj.GetString("flag");
				string value = itemObj.GetString("value");
				string picName = itemObj.GetString("picName");

				items2.Add(new ItemData(itemName, offset, flag, value, picName));
			}

			return items2;
		}

		private class LocationData
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

		private class ItemData
		{
			public string Item { get; }
			public int Offset { get; }
			public string Flag { get; }
			public string Value { get; }
			public string PicName { get; }

			public ItemData(string item, int offset, string flag, string value, string picPath)
			{
				Item = item;
				Offset = offset;
				Flag = flag;
				Value = value;
				PicName = picPath;
			}
		}
	}
}