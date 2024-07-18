using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ArchipelagoRandomizer.ItemHandler;

namespace ArchipelagoRandomizer
{
	internal class MessageBoxHandler : MonoBehaviour
	{
		private static MessageBoxHandler instance;
		private static Color playerNameColor;
		private static Color itemNameProgressiveColor;
		private static Color itemNameUsefulColor;
		private static EntityHUDData hudData;

		private readonly List<MessageBox> messageBoxQueue = new();
		private MessageBox currentMessageBox;

		public static MessageBoxHandler Instance { get { return instance; } }
		private bool CanShowMessageBox
		{
			get
			{
				return currentMessageBox == null || (currentMessageBox != null && !currentMessageBox.IsActive);
			}
		}

		public enum MessageType
		{
			Unspecified,
			ReceivedFromSelf,
			ReceivedFromSomeone,
			Sent
		}

		public void ShowMessageBox(MessageData data)
		{
			MessageBox messageBox = new(data);
			messageBoxQueue.Add(messageBox);
		}

		public void HideMessageBoxes(bool clearQueue = true)
		{
			currentMessageBox?.Hide();

			if (clearQueue)
				messageBoxQueue.Clear();
		}

		private void Awake()
		{
			instance = this;
			ColorUtility.TryParseHtmlString("#fafad2", out playerNameColor);
			ColorUtility.TryParseHtmlString("#5e5674", out itemNameProgressiveColor);
			ColorUtility.TryParseHtmlString("#526294", out itemNameUsefulColor);

			if (hudData == null)
				hudData = Resources.Load<EntityHUDData>("HUDs/XboneHUDData");

			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			if (CanShowMessageBox && messageBoxQueue.Count > 0)
			{
				// Show oldest message box in the queue (is always at index 0)
				currentMessageBox = messageBoxQueue[0];
				currentMessageBox.Show();
				messageBoxQueue.RemoveAt(0);
			}
		}

		private class MessageBox
		{
			private ItemMessageBox messageBox;

			public bool IsActive
			{
				get
				{
					return messageBox != null && messageBox.IsActive;
				}
			}
			private MessageData Data { get; }

			public MessageBox(MessageData data)
			{
				data.Message = string.IsNullOrEmpty(data.Message) ? GetMessageForItem(data) : data.Message;
				Data = data;
			}

			public void Show()
			{
				if (messageBox == null)
				{
					messageBox = OverlayWindow.GetPooledWindow(hudData.GetItemBox);
					SetIconTexture();
					messageBox._text.StringText = new StringHolder.OutString(Data.Message);
					Plugin.StartRoutine(SetTextColors());
					ResizeBox();
					messageBox.timer = Data.DisplayTime;
					messageBox.countdown = Data.DisplayTime > 0;
				}

				if (messageBox._tweener != null)
					messageBox._tweener.Show(true);
				else
					messageBox.gameObject.SetActive(true);
			}

			public void Hide()
			{
				if (messageBox == null)
					return;

				if (messageBox._tweener != null)
					messageBox._tweener.Hide(true);
				else
					messageBox.gameObject.SetActive(false);
			}

			private string GetMessageForItem(MessageData data)
			{
				string message = "You should not be seeing this. Please report!";
				string itemName = !string.IsNullOrEmpty(data.ItemName) ? data.ItemName : "Unknown Item";
				string playerName = !string.IsNullOrEmpty(data.PlayerName) ? data.PlayerName : "Unknown Player";

				switch (data.MessageType)
				{
					case MessageType.ReceivedFromSelf:
						message = $"You found your own {itemName}{GetCountText(data.Item)}";
						break;
					case MessageType.ReceivedFromSomeone:
						if (playerName == "Server")
							message = $"The server gave you {itemName}{GetCountText(data.Item)}";
						else
							message = $"{playerName} found your {itemName}{GetCountText(data.Item)}";
						break;
					case MessageType.Sent:
						message = $"You found {itemName} for {playerName}!";
						break;
				}

				return message;
			}

			private string GetCountText(ItemData.Item item)
			{
				int count = ItemHandler.Instance.GetItemCount(item, out bool isLevelItem);

				if (count == 0)
					return "!";

				if (isLevelItem)
					return $"! You're now at level {count}!";

				if (item.Type == ItemType.Key)
					return $"! You have {count} key{(count > 1 ? "s" : "")} for this dungeon.";

				return $"! You have {count} of them.";
			}

			private void SetIconTexture()
			{
				if (string.IsNullOrEmpty(Data.ItemName))
				{
					Texture2D disconnectedTex = ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/Disconnected.png");
					messageBox.texture = disconnectedTex;
					messageBox.mat.mainTexture = disconnectedTex;
					return;
				}

				string iconName = Data.Item != null ? Data.Item.IconName : "Custom/APUseful";

				// Increment melee icon from stick
				if (Data.Item != null && Data.Item.Type == ItemType.Melee)
				{
					int level = ModCore.Utility.GetPlayer().GetStateVariable("melee");

					if (level > 0)
						iconName = $"Melee{level}";
				}

				bool isCustomIcon = iconName.StartsWith("Custom");
				string iconPath = !isCustomIcon ?
					$"Items/ItemIcon_{iconName}" :
					$"{PluginInfo.PLUGIN_NAME}/Assets/{iconName.Substring(iconName.LastIndexOf("/"))}.png";
				Texture2D texture = !isCustomIcon ? Resources.Load(iconPath) as Texture2D : ModCore.Utility.GetTextureFromFile(iconPath);

				if (texture == null)
					return;

				messageBox.texture = texture;
				messageBox.mat.mainTexture = texture; ;
			}

			/// <summary>
			/// Colorizes some parts of the message text, such as item or player names
			/// </summary>
			private IEnumerator SetTextColors()
			{
				// If item name isn't in message, do nothing
				if (string.IsNullOrEmpty(Data.ItemName) || !Data.Message.Contains(Data.ItemName))
					yield break;

				// Arbitrary delay due to issues with setting message box text property
				yield return new WaitForEndOfFrame();

				// Remove spaces from message so color indices don't get thrown off by them, as
				// Each space has one index for color
				string messageWithoutSpaces = Data.Message.Replace(" ", "");
				string itemNameWithoutSpaces = Data.ItemName.Replace(" ", "");
				Color[] meshColors = messageBox._text.mesh.colors;
				int startItemNameIndex = messageWithoutSpaces.IndexOf(itemNameWithoutSpaces);

				// Multiply index by 4 as each character has 4 color indices
				ReplaceMeshColors(meshColors, startItemNameIndex * 4, itemNameWithoutSpaces.Length * 4, itemNameProgressiveColor);

				// If a player's name is in message
				if (Data.Message.Contains(Data.PlayerName))
				{
					// Remove spaces from message so color indices don't get thrown off by them, as
					// Each space has one index for color
					string playerNameWithoutSpaces = Data.PlayerName.Replace(" ", "");
					int startPlayerNameIndex = messageWithoutSpaces.IndexOf(playerNameWithoutSpaces);

					// Multiply index by 4 as each character has 4 color indices
					ReplaceMeshColors(meshColors, startPlayerNameIndex * 4, playerNameWithoutSpaces.Length * 4, playerNameColor);
				}

				messageBox._text.mesh.colors = meshColors;
			}

			/// <summary>
			/// Replaces the TextMesh's colors
			/// </summary>
			/// <param name="colors">The array of Colors</param>
			/// <param name="startIndex">The start index of the range to change</param>
			/// <param name="length">The length of the range to change</param>
			/// <param name="color">The color to change to</param>
			private void ReplaceMeshColors(Color[] colors, int startIndex, int length, Color color)
			{
				int endIndex = Mathf.Min(startIndex + length, colors.Length);

				for (int i = startIndex; i < endIndex; i++)
					colors[i] = color;
			}

			/// <summary>
			/// Enlarges the box to fit text, but doesn't shrink it below 3 lines
			/// </summary>
			private void ResizeBox()
			{
				Vector2 scaledTextSize = messageBox._text.ScaledTextSize;
				Vector3 vector = messageBox._text.transform.localPosition - messageBox.backOrigin;
				scaledTextSize.y += Mathf.Abs(vector.y) + messageBox._border;
				scaledTextSize.y = Mathf.Max(messageBox.minSize.y, scaledTextSize.y);
				scaledTextSize.x = messageBox._background.ScaledSize.x;
				messageBox._background.ScaledSize = scaledTextSize;
			}
		}

		public struct MessageData
		{
			public string Message { get; set; }
			public ItemData.Item Item { get; set; }
			public string ItemName { get; set; }
			public string PlayerName { get; set; }
			public MessageType MessageType { get; set; }
			public float DisplayTime { get; set; } = 3f;

			public MessageData() { }
		}
	}
}