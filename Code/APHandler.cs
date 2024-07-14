using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using ModCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer
{
	public class APHandler
	{
		private static APHandler instance;
		private const int baseId = 238492834;
		private List<ScoutedItemInfo> scoutedItems;

		public static APHandler Instance { get { return instance; } }
		public static ArchipelagoSession Session { get; private set; }
		public static Dictionary<string, object> slotData;
		public PlayerInfo CurrentPlayer { get; private set; }
		public static bool IsConnected { get { return Session != null; } }

		public APHandler()
		{
			instance = this;
		}

		public bool TryCreateSession(string url, string slot, string password, out string message)
		{
			if (Session != null)
			{
				Session.MessageLog.OnMessageReceived -= OnReceiveMessage;
			}
			try
			{
				Session = ArchipelagoSessionFactory.CreateSession(url);
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return false;
			}

			LoginResult result;

			try
			{
				result = Session.TryConnectAndLogin("Ittle Dew 2", slot, ItemsHandlingFlags.AllItems, password: password, requestSlotData: true);
			}
			catch (Exception ex)
			{
				result = new LoginFailure(ex.GetBaseException().Message);
			}

			if (!result.Successful)
			{
				LoginFailure failure = (LoginFailure)result;
				string errorMessage = $"Failed to connect to {url} as {slot}:";
				foreach (string error in failure.Errors)
				{
					errorMessage += $"\n    {error}";
				}
				foreach (ConnectionRefusedError error in failure.ErrorCodes)
				{
					errorMessage += $"\n    {error}";
				}
				message = errorMessage;
				return false;
			}

			OnConnected((LoginSuccessful)result);
			var loginSuccess = (LoginSuccessful)result;
			message = "Successfully connected!\nNow that you are connected, you can use !help to list commands to run via the server.";

			return true;
		}

		public void LocationChecked(int offset)
		{
			if (!IsConnected)
				return;

			Session.Locations.CompleteLocationChecks(baseId + offset);
		}

		public void SyncItemsWithServer()
		{
			IDataSaver itemsObtainedSaver = ModCore.Plugin.MainSaver.GetSaver("/local/archipelago/itemsObtained");
			int itemsObtainedCount = itemsObtainedSaver.LoadInt("count");
			var apItemsObtained = Session.Items.AllItemsReceived;
			List<ItemInfo> itemsAlreadySent = new();

			if (apItemsObtained.Count > itemsObtainedCount)
			{
				Plugin.Log.LogInfo($"Save file is out of sync with Archipelago server!\nReceiving {apItemsObtained.Count - itemsObtainedCount} item(s)!");

				foreach (ItemInfo item in apItemsObtained)
				{
					if (itemsAlreadySent.Contains(item))
						continue;

					int countInSaveFile = itemsObtainedSaver.LoadInt(item.ItemDisplayName);

					if (apItemsObtained.Where(x => x.ItemDisplayName == item.ItemDisplayName).ToList().Count > countInSaveFile)
					{
						int itemOffset = (int)item.ItemId - baseId;
						Plugin.StartRoutine(ItemRandomizer.Instance.ItemReceived(itemOffset, item.ItemDisplayName, item.Player.Name));
						itemsAlreadySent.Add(item);
						Plugin.Log.LogInfo($"{item.ItemDisplayName} was not synced, but it is now!");
					}
				}
			}
		}

		private void OnConnected(LoginSuccessful loginSuccess)
		{
			slotData = loginSuccess.SlotData;
			CurrentPlayer = Session.Players.GetPlayerInfo(Session.ConnectionInfo.Slot);

			Session.MessageLog.OnMessageReceived += OnReceiveMessage;
			Session.Locations.CheckedLocationsUpdated += OnLocationChecked;
			Session.Items.ItemReceived += OnReceivedItem;
			Session.Socket.SocketClosed += OnDisconnected;

			Plugin.Log.LogInfo($"Successfully connected to Archipelago server!");
			ScoutLocations();
		}

		private void OnDisconnected(string reason)
		{
			if (ItemRandomizer.Instance.IsActive)
				Plugin.StartRoutine(ItemRandomizer.Instance.OnDisconnected());

			Session = null;
		}

		private void OnLocationChecked(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
		{
			long id = newCheckedLocations[newCheckedLocations.Count - 1];
			string locationName = Session.Locations.GetLocationNameFromId(id);
			ScoutedItemInfo item = scoutedItems.FirstOrDefault(x => x.LocationId == id);

			// If sending item
			if (item != null && item.Player.Slot != CurrentPlayer.Slot)
				Plugin.StartRoutine(ItemRandomizer.Instance.ItemSent(item.ItemDisplayName, item.Player.Name));

			Plugin.Log.LogInfo($"Checked location: {locationName}");
		}

		private void OnReceivedItem(ReceivedItemsHelper helper)
		{
			ItemInfo receivedItem = Session.Items.AllItemsReceived[Session.Items.AllItemsReceived.Count - 1];
			int itemOffset = (int)receivedItem.ItemId - baseId;
			Plugin.StartRoutine(ItemRandomizer.Instance.ItemReceived(itemOffset, receivedItem.ItemDisplayName, receivedItem.Player.Name));
		}

		private void ScoutLocations()
		{
			scoutedItems = new();

			Session.Locations.ScoutLocationsAsync((scoutResult) =>
			{
				foreach (ScoutedItemInfo item in scoutResult.Values)
				{
					scoutedItems.Add(item);
				}
			}, Session.Locations.AllLocations.ToArray());
		}

		private void OnReceiveMessage(LogMessage message)
		{
			DebugMenuManager.LogToConsole(message.ToString());
		}
	}
}