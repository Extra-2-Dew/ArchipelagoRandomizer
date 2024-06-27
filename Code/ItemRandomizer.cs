using SmallJson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class ItemRandomizer
	{
		private const int baseId = 238492834;
		private static ItemRandomizer instance;
		private readonly List<LocationData> locationData;
		private readonly List<ItemData> itemData;

		public static ItemRandomizer Instance { get { return instance; } }
		public bool HasInitialized { get; private set; }

		public ItemRandomizer(bool newFile)
		{
			instance = this;

			// Parse data JSON
			locationData = ParseLocationJson();
			itemData = ParseItemJson();
			HasInitialized = locationData != null && itemData != null;

			if (newFile && HasInitialized)
				SetupNewFile();
		}

		internal void SetupNewFile()
		{
			SaverOwner saver = ModCore.Plugin.MainSaver;
			saver.LocalStorage.GetLocalSaver("settings").SaveInt("hideCutscenes", 1);
			saver.SaveAll();
		}

		public void HandleItemReplacement(RandomizedItemData itemData)
		{
			if (!HasInitialized || itemData.ItemId == null || itemData.Entity == null)
				return;

			Plugin.Log.LogInfo($"Obtained {itemData.ItemId.name}! Holy crud!!");

			ItemData randomItem = this.itemData[Random.Range(0, this.itemData.Count - 1)];

			int currentValue = itemData.Entity.GetStateVariable(randomItem.Flag);
			itemData.Entity.SetStateVariable(randomItem.Flag, randomItem.Value.StartsWith("+") ? currentValue++ : int.Parse(randomItem.Value));
			itemData.ItemId._itemGetPic = $"Items/ItemIcon_{char.ToUpper(randomItem.Flag[0]) + randomItem.Flag.Substring(1)}";
			itemData.ItemId._itemGetString = $"You got {randomItem.Item}! Holy crud!!";
			itemData.ItemId._showMode = ItemId.ShowMode.Normal;
		}

		//public Item HandleItemReplacement(Item item, Entity entity)
		//{
		//	if (!HasInitialized || item == null || entity == null)
		//		return item;

		//	Plugin.Log.LogInfo($"Obtained {item.name}! Holy crud!!");

		//	ItemData randomItem = itemData[Random.Range(0, itemData.Count - 1)];

		//	int currentValue = entity.GetStateVariable(randomItem.Flag);
		//	entity.SetStateVariable(randomItem.Flag, randomItem.Value.StartsWith("+") ? currentValue++ : int.Parse(randomItem.Value));
		//	item.ItemId._itemGetPic = $"Items/ItemIcon_{char.ToUpper(randomItem.Flag[0]) + randomItem.Flag.Substring(1)}";
		//	item.ItemId._itemGetString = $"You got {randomItem.Item}! Holy crud!!";
		//	item.ItemId._showMode = ItemId.ShowMode.Normal;
		//	return item;
		//}

		private ItemData GetItemForLocation(LocationData location)
		{
			return null;
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