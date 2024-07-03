using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using ModCore;
using System;

namespace ArchipelagoRandomizer
{
	public class APHandler
	{
		private static APHandler instance;
		private const int baseId = 238492834;
		private ArchipelagoSession session;
		private PlayerInfo currentPlayer;

		public static APHandler Instance { get { return instance; } }
		public static ArchipelagoSession Session { get { return instance.session; } }
		public PlayerInfo CurrentPlayer { get { return currentPlayer; } }

		public APHandler()
		{
			instance = this;
		}

		public bool TryCreateSession(string url, string slot, string password, out string message)
		{
			if (session != null)
			{
				session.MessageLog.OnMessageReceived -= OnReceiveMessage;
			}
			try
			{
				session = ArchipelagoSessionFactory.CreateSession(url);
			}
			catch (Exception ex)
			{
				message = ex.Message;
				return false;
			}

			LoginResult result;

			try
			{
				result = session.TryConnectAndLogin("Ittle Dew 2", slot, ItemsHandlingFlags.AllItems, password: password);
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

			var loginSuccess = (LoginSuccessful)result;
			currentPlayer = session.Players.GetPlayerInfo(session.ConnectionInfo.Slot);
			session.MessageLog.OnMessageReceived += OnReceiveMessage;
			message = "Successfully connected!\n" +
				"Now that you are connected, you can use !help to list commands to run via the server.";
			session.Items.ItemReceived += OnReceivedItem;
			return true;
		}

		private void OnReceivedItem(ReceivedItemsHelper helper)
		{
			ItemInfo receivedItem = session.Items.AllItemsReceived[session.Items.AllItemsReceived.Count - 1];
			int itemOffset = (int)receivedItem.ItemId - baseId;
			ItemRandomizer.Instance.ItemReceived(itemOffset, receivedItem.Player.Name);
		}

		public void LocationChecked(int offset)
		{
			if (session == null)
			{
				Plugin.Log.LogError("Attempted to interact with Archipelago server, but no session has been started yet!");
				return;
			}

			int id = baseId + offset;
			session.Locations.CompleteLocationChecksAsync((completed) =>
			{
				if (completed)
				{
					string locationName = session.Locations.GetLocationNameFromId(id);
					Plugin.Log.LogInfo($"Checked location: {locationName} ({offset})");
				}
			}, id);
		}

		private void OnReceiveMessage(LogMessage message)
		{
			DebugMenuManager.LogToConsole(message.ToString());
		}
	}
}