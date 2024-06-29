using SmallJson;
using System.Collections.Generic;
using System.Linq;

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

		//ap /connect localhost:38281 WyrmID

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
			ShowItemSentHud(itemName, playerName);
		}

		public void ItemReceived(int offset)
		{
			ItemData item = itemData.Find(x => x.Offset == offset);

			if (item == null)
				return;

			ShowItemGetHud(item);
		}

		private void ShowItemSentHud(string itemName, string playerName)
		{
			string message = $"You found {itemName} for {playerName}!";
			string picPath = "ArchipelagoIcon";

			ShowHud(message, picPath);
		}

		private void ShowItemGetHud(ItemData itemData)
		{
			string message = $"Someone sent you {itemData.Item}!";
			string picPath = "ItemPic";

			ShowHud(message, picPath);
		}

		private void ShowHud(string message, string picPath)
		{
			EntityHUD currentHud = EntityHUD.GetCurrentHUD();

			if (currentHud.currentMsgBox != null && currentHud.currentMsgBox.IsActive)
				currentHud.currentMsgBox.Hide(true);

			currentHud.currentMsgBox = OverlayWindow.GetPooledWindow(currentHud._data.GetItemBox);
			currentHud.currentMsgBox.Show(picPath, new StringHolder.OutString(message));
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

				items2.Add(new ItemData(itemName, offset, flag, value));
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

			public ItemData(string item, int offset, string flag, string value)
			{
				Item = item;
				Offset = offset;
				Flag = flag;
				Value = value;
			}
		}
	}
}