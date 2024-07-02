using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using ModCore;
using System;

namespace ArchipelagoRandomizer
{
	public class APHandler
	{
		private static APHandler instance;
		private const int baseId = 238492834;
		private ArchipelagoSession session;

		public static APHandler Instance { get { return instance; } }
		public static ArchipelagoSession Session { get { return instance.session; } }

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
			session.MessageLog.OnMessageReceived += OnReceiveMessage;
			message = "Successfully connected!\n" +
				"Now that you are connected, you can use !help to list commands to run via the server.";
			session.Items.ItemReceived += OnReceivedItem;
			return true;
		}

		private void OnReceivedItem(ReceivedItemsHelper helper)
		{
			//APCommand.Instance.Test(new[] { "test message" });
			APCommand.Instance.Test(new[] { "test message" });
			//ItemInfo mostRecentItem = session.Items.AllItemsReceived[session.Items.AllItemsReceived.Count - 1];
			//PlayerInfo playerInfo = session.Players.GetPlayerInfo(session.ConnectionInfo.Slot);
			//int itemOffset = (int)mostRecentItem.ItemId - baseId;

			//if (mostRecentItem.Player.Name == playerInfo.Name || mostRecentItem.Player.Alias == playerInfo.Alias)
			//{
			//	//ItemRandomizer.Instance.ItemReceived(itemOffset);
			//	APCommand.Instance.Test(new[] { "test message" });
			//	Plugin.Log.LogInfo($"Received item {mostRecentItem.ItemDisplayName}!");
			//}
			//else
			//{
			//	ItemRandomizer.Instance.ItemSent(mostRecentItem.ItemDisplayName, mostRecentItem.Player.Name);
			//	Plugin.Log.LogInfo($"Sent {mostRecentItem.ItemDisplayName} to {mostRecentItem.Player.Name}!");
			//}
		}

		public void LocationChecked(int offset)
		{
			if (session == null)
			{
				Plugin.Log.LogError("Attempted to interact with Archipelago server, but no session has been started yet!");
				return;
			}

			int id = baseId + offset;
			session.Locations.CompleteLocationChecks(id);
			string locationName = session.Locations.GetLocationNameFromId(id);
			Plugin.Log.LogInfo($"Checked location: {locationName} ({offset})");
			//session.Locations.CompleteLocationChecksAsync((completed) =>
			//{
			//	if (completed)
			//	{
			//		string locationName = session.Locations.GetLocationNameFromId(id);
			//		Plugin.Log.LogInfo($"Checked location: {locationName} ({offset})");
			//	}
			//}, id);
		}

		private void OnReceiveMessage(LogMessage message)
		{
			DebugMenuManager.LogToConsole(message.ToString());
		}
	}
}