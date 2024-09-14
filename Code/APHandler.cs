using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using ModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class APHandler
	{
		private static APHandler instance;
		private static List<ScoutedItemInfo> scoutedItems;
		private static Dictionary<string, object> slotData;
		private const int baseId = 238492834;

		public static APHandler Instance { get { return instance; } }
		public event OnDisconnectFunc OnDisconnect;
		public PlayerInfo CurrentPlayer
		{
			get
			{
				if (Session == null)
					return null;

				return Session.Players.GetPlayerInfo(Session.ConnectionInfo.Slot);
			}
		}
		public bool IsConnected { get { return Session != null && Session.Socket.Connected; } }
		public ArchipelagoSession Session { get; private set; }

		public APHandler()
		{
			instance = this;
		}

		public static T GetSlotData<T>(string key)
		{
			if (slotData == null || !slotData.TryGetValue(key, out object value))
			{
				Plugin.Log.LogError($"No slot data with key '{key}' was found. Returning null!");
				return default(T);
			}

			return (T)value;
		}

		public bool TryCreateSessionAndConnect(ItemRandomizer.APFileData apFileData, out string errorMessage)
		{
			if (apFileData == null)
			{
				errorMessage = "You must specify server, port, and slot name to connect to Archipelago server!";
				return false;
			}

			string url = $"{apFileData.Server}:{apFileData.Port}";

			return
				TryCreateSession(url, out errorMessage) &&
				TryConnect(url, apFileData.SlotName, apFileData.Password, out errorMessage);
		}

		public bool TryCreateSessionAndConnect(string url, string slot, string password, out string errorMessage)
		{
			return
				TryCreateSession(url, out errorMessage) &&
				TryConnect(url, slot, password, out errorMessage);
		}

		public void Disconnect()
		{
			if (!IsConnected)
				return;

			Session.Socket.Disconnect();
			OnDisconnected("Manual disconnection");
		}

		public void LocationChecked(int offset)
		{
			if (!IsConnected)
				return;

			Action locationCheckedDelegate = () =>
			{
				Session.Locations.CompleteLocationChecksAsync((bool success) =>
				{
					long id = baseId + offset;
					string locationName = Session.Locations.GetLocationNameFromId(id);
					ScoutedItemInfo item = scoutedItems.FirstOrDefault(x => x.LocationId == id);

					// If sending item
					if (item != null && item.Player.Slot != CurrentPlayer.Slot)
						Plugin.StartRoutine(ItemRandomizer.Instance.ItemSent(item.ItemDisplayName, item.Player.Name));

					ModCore.Plugin.MainSaver.SaveLocal();
					Plugin.Log.LogInfo($"Checked location: {locationName}");
				}, baseId + offset);
			};

			locationCheckedDelegate.BeginInvoke(new AsyncCallback((IAsyncResult ar) =>
			{
				locationCheckedDelegate.EndInvoke(ar);
			}), null);
		}

		public void SyncItemsWithServer()
		{
			IDataSaver itemsObtainedSaver = ModCore.Plugin.MainSaver.GetSaver("/local/archipelago/itemsObtained");
			int itemsObtainedCount = itemsObtainedSaver.LoadInt("count");
			var apItemsObtained = Session.Items.AllItemsReceived;
			var itemsLeftToSync = apItemsObtained.ToList();

			if (apItemsObtained.Count > itemsObtainedCount)
			{
				Plugin.Log.LogInfo($"Save file is out of sync with Archipelago server!\nReceiving {apItemsObtained.Count - itemsObtainedCount} item(s)!");

				foreach (ItemInfo item in apItemsObtained)
				{
					int countInSaveFile = itemsObtainedSaver.LoadInt(item.ItemDisplayName); // THIS STAYS THE SAME FOR ENTIRE LOOP
					int countInServer = itemsLeftToSync.Where(x => x.ItemDisplayName == item.ItemDisplayName).Count() - countInSaveFile;

					if (countInServer > 0)
					{
						int itemOffset = (int)item.ItemId - baseId;
						Plugin.StartRoutine(ItemRandomizer.Instance.ItemReceived(itemOffset, item.ItemDisplayName, item.Player.Name));
						itemsLeftToSync.Remove(item);
						Plugin.Log.LogInfo($"{item.ItemDisplayName} was not synced, but it is now!");
					}
				}
			}
		}

		public ScoutedItemInfo GetScoutedItemInfo(ItemRandomizer.LocationData.Location forLocation)
		{
			if (forLocation == null)
				return null;

			return scoutedItems.Find(x => x.LocationId - forLocation.Offset == baseId);
		}

		public void SetPosition(Vector2 position)
		{
			if (!IsConnected)
			{
				return;
			}

			var key = $"id2.pos.{CurrentPlayer.Slot}";
			var value = $"{(int)position.x},{(int)position.y}";

			ThreadPool.QueueUserWorkItem((_) =>
			{
				Session.DataStorage[key] = value;
			});
		}

		public void SetLevelName(string levelName)
		{
			if (!IsConnected)
			{
				return;
			}

			var key = $"id2.levelName.{CurrentPlayer.Slot}";
			var value = levelName;

			ThreadPool.QueueUserWorkItem((_) =>
			{
				Session.DataStorage[key] = value;
			});
		}

		private bool TryCreateSession(string url, out string errorMessage)
		{
			if (Session != null)
				Session.MessageLog.OnMessageReceived -= OnReceiveMessage;

			try
			{
				Session = ArchipelagoSessionFactory.CreateSession(url);
			}
			catch (Exception ex)
			{
				errorMessage = $"Failed to create Archipelago session!\nError: {ex.Message}";
				Plugin.Log.LogError(errorMessage);
				return false;
			}

			errorMessage = string.Empty;
			return true;
		}

		private bool TryConnect(string url, string slot, string password, out string errorMessage)
		{
			LoginResult result;

			try
			{
				result = Session.TryConnectAndLogin("Ittle Dew 2", slot, ItemsHandlingFlags.AllItems, password: password, requestSlotData: true);
			}
			catch (Exception ex)
			{
				result = new LoginFailure(ex.GetBaseException().Message);
			}

			// If failed to connect
			if (!result.Successful)
			{
				LoginFailure failure = (LoginFailure)result;
				errorMessage = $"Failed to connect to {url} as {slot}:";

				foreach (string error in failure.Errors)
					errorMessage += $"\n    {error}";
				foreach (ConnectionRefusedError error in failure.ErrorCodes)
					errorMessage += $"\n    {error}";

				Plugin.Log.LogError(errorMessage);
				return false;
			}

			// If connection successful
			LoginSuccessful success = (LoginSuccessful)result;
			Plugin.Log.LogInfo($"Connected to {url} as {slot} on team {success.Team}! Have fun!");
			OnConnected(success);
			errorMessage = string.Empty;
			return true;
		}

		private void OnConnected(LoginSuccessful loginSuccess)
		{
			slotData = loginSuccess.SlotData;

			Session.MessageLog.OnMessageReceived += OnReceiveMessage;
			Session.Items.ItemReceived += OnReceivedItem;
			Session.Socket.SocketClosed += OnDisconnected;

			ScoutLocations();
		}

		private void OnDisconnected(string reason)
		{
			if (ItemRandomizer.IsActive)
				Plugin.StartRoutine(ItemRandomizer.Instance.OnDisconnected());

			Session.MessageLog.OnMessageReceived -= OnReceiveMessage;
			Session.Items.ItemReceived -= OnReceivedItem;
			Session.Socket.SocketClosed -= OnDisconnected;
			Session = null;

			OnDisconnect?.Invoke();

			Plugin.Log.LogInfo("Disconnected from Archipelago server!");
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

		public delegate void OnDisconnectFunc();
	}
}