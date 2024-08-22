using Archipelago.MultiClient.Net.Packets;
using ModCore;

namespace ArchipelagoRandomizer
{

	public class APCommand
	{
		private static APCommand instance;
		public static APCommand Instance { get { return instance; } }

		public APCommand()
		{
			instance = this;
		}

		public void AddCommands()
		{
			DebugMenuManager.AddCommands(this);
		}

		[DebugMenuCommand(commandName: "archipelago", commandAliases: ["ap"], caseSensitive: true)]
		private void SendAPCommand(string[] args)
		{
			if (args.Length == 0)
			{
				DebugMenuManager.LogToConsole("USAGE:\n" +
					"  ap !hint {item}: Hint the location of an item\n" +
					"  ap !release: Release all items remaining in your world to other worlds (if you have permission)\n" +
					"  ap !collect: Receive all items that belong to your world (if you have permission)\n" +
					"You can also simply type to chat with other players.", DebugMenuManager.TextColor.Error);
				return;
			}

			if (!APHandler.Instance.IsConnected)
			{
				DebugMenuManager.LogToConsole("No session active. Please connect first!", DebugMenuManager.TextColor.Error);
				return;
			}

			string combinedArgs = string.Join(" ", args).TrimEnd();

			if (combinedArgs == "")
				return;

			APHandler.Instance.Session.Socket.SendPacket(new SayPacket() { Text = combinedArgs });
		}
	}
}