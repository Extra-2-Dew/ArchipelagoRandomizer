using Archipelago.MultiClient.Net.Packets;
using ModCore;

namespace ArchipelagoRandomizer
{

	public class APCommand
	{
		private static APCommand instance;
		public static APCommand Instance { get { return instance; } }

		private APCommand()
		{
			instance = this;
			DebugMenuManager.AddCommands(this);
		}

		[DebugMenuCommand("test")]
		public void Test(string[] args)
		{
			Plugin.Instance.StartCoroutine(Plugin.Test(args[0]));
			//Entity entity = GameObject.Find("PlayerEnt").GetComponent<Entity>();
			//Debug.Log(entity == null);
			//Item item = GameObject.Find("Dungeon_Chest").GetComponent<SpawnItemEventObserver>()._itemPrefab;
			//Debug.Log(item == null);
			//item.ItemId._itemGetPic = "ArchipelagoIcon";
			//item.ItemId._itemGetString = args[0];
			//item.ItemId._showMode = ItemId.ShowMode.Normal;


			//currentHud.currentMsgBox.Show(item.ItemId);
			//currentHud.currentMsgBox.Show("Items/ItemIcon_Fullpaper", currentHud.currentMsgBox._textLoader.GetStrings().GetFullString("item_PieceOfPaper_get"));
			//currentHud.currentMsgBox._text.Text = args[0];
			//EntityHUD.GetCurrentHUD().currentMsgBox.Show(item.ItemId);
			//currentHud.currentMsgBox.Show(item.ItemId.ItemGetPic, new StringHolder.OutString(item.ItemId.ItemGetString));
			//currentHud.GotItem(entity, item);
		}

		[DebugMenuCommand(commandName: "archipelago", commandAliases: ["ap"], caseSensitive: true)]
		private void SendAPCommand(string[] args)
		{
			if (args.Length == 0)
			{
				DebugMenuManager.LogToConsole("USAGE:\n" +
					"  ap /connect {server:port} {slot} {password}: Connect to an ArchipelagoHandler server\n" +
					"  ap !hint {item}: Hint the location of an item\n" +
					"  ap !release: Release all items remaining in your world to other worlds (if you have permission)\n" +
					"  ap !collect: Receive all items that belong to your world (if you have permission)\n" +
					"You can also simply type to chat with other players.", DebugMenuManager.TextColor.Error);
				return;
			}
			if (args[0] == "/connect")
			{
				if (args.Length < 3)
				{
					DebugMenuManager.LogToConsole("USAGE:\n" +
						"  ap /connect {server:port} {slot} {password (if needed)}\n" +
						"Examples:" +
						"ap /connect localhost:38281 PlayerIttle\n" +
						"ap /connect archipelago.gg:12345 PlayerIttle mYPassWord", DebugMenuManager.TextColor.Error);
					return;
				}
				string server = args[1];
				string slot = args[2];
				string password = "";
				if (args.Length >= 4) password = args[3];
				if (APHandler.Instance.TryCreateSession(server, slot, password, out string message))
				{
					DebugMenuManager.LogToConsole(message, DebugMenuManager.TextColor.Success);
				}
				else
				{
					DebugMenuManager.LogToConsole(message, DebugMenuManager.TextColor.Error);
				}

				return;
			}

			if (APHandler.Session == null)
			{
				DebugMenuManager.LogToConsole("No session active. Please connect with 'ap /connect' first.", DebugMenuManager.TextColor.Error);
				return;
			}

			string combinedArgs = string.Join(" ", args).TrimEnd();

			if (combinedArgs == "") return;

			APHandler.Session.Socket.SendPacket(new SayPacket() { Text = combinedArgs });
		}

	}
}
