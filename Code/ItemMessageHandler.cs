using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	internal class ItemMessageHandler
	{
		private static readonly Color playerNameColor;
		private static readonly Color itemNameProgressiveColor;
		private static readonly Color itemNameUsefulColor;
		private readonly List<MessageBox> queuedMessageBoxes = new();
		private MessageBox currentMessageBox;

		public enum MessageType
		{
			ReceivedFromSelf,
			ReceivedFromSomeone,
			Sent
		}

		static ItemMessageHandler()
		{
			ColorUtility.TryParseHtmlString("#fafad2", out playerNameColor);
			ColorUtility.TryParseHtmlString("#5e5674", out itemNameProgressiveColor);
			ColorUtility.TryParseHtmlString("#526294", out itemNameUsefulColor);
		}

		public IEnumerator ShowMessageBox(MessageType messageType, string itemName, string playerName, string iconName)
		{
			// Arbitrary delay due to issues with setting message box text property
			yield return new WaitForEndOfFrame();
			Plugin.Log.LogInfo(iconName);

			string iconPath = $"Items/ItemIcon_{iconName}";

			// Show message box
			if (currentMessageBox == null)
			{
				currentMessageBox = new(messageType, itemName, playerName, iconPath);
				currentMessageBox.Show();
			}
			// Queue message box
			else
			{
				MessageBox queuedBox = new(messageType, itemName, playerName, iconPath);
				queuedMessageBoxes.Add(queuedBox);

				// Wait until no message box is shown
				while (currentMessageBox.IsActive)
					yield return null;

				// Show oldest message box in queue (always at index 0)
				queuedMessageBoxes[0].Show();
				currentMessageBox = queuedMessageBoxes[0];
				queuedMessageBoxes.RemoveAt(0);
			}
		}

		private class MessageBox
		{
			private ItemMessageBox messageBox;

			/// <summary>
			/// Is the message box currently shown?
			/// </summary>
			public bool IsActive
			{
				get
				{
					return messageBox != null && messageBox.IsActive;
				}
			}
			private string Message { get; }
			private string IconPath { get; }
			private float DisplayTime { get; }
			private string ItemName { get; }
			private string PlayerName { get; }

			/// <summary>
			/// Creates a new MessageBox
			/// </summary>
			/// <param name="messageType">The type of the message it should show</param>
			/// <param name="itemName">The name of the item</param>
			/// <param name="playerName">The name of the involved player</param>
			/// <param name="iconPath">The full Resources path to the original icon, or the relative path to the custom icon</param>
			/// <param name="displayTime">How long should the message stay up for (in seconds)?</param>
			public MessageBox(MessageType messageType, string itemName, string playerName, string iconPath, float displayTime = 3f)
			{
				Message = GetMessage(messageType, itemName, playerName);
				IconPath = iconPath;
				DisplayTime = displayTime;
				ItemName = itemName;
				PlayerName = playerName;
			}

			/// <summary>
			/// Shows the message box
			/// </summary>
			public void Show()
			{
				if (messageBox == null)
				{
					EntityHUD hud = EntityHUD.GetCurrentHUD();
					messageBox = OverlayWindow.GetPooledWindow(hud._data.GetItemBox);
					SetIconTexture();
					messageBox._text.StringText = new StringHolder.OutString(Message);
					Plugin.StartRoutine(SetTextColors());
					ResizeBox();
					messageBox.timer = DisplayTime;
					messageBox.countdown = DisplayTime > 0;
				}

				if (messageBox._tweener != null)
					messageBox._tweener.Show(true);
				else
					messageBox.gameObject.SetActive(true);
			}

			/// <summary>
			/// Hides the message box
			/// </summary>
			public void Hide()
			{
				if (messageBox._tweener != null)
					messageBox._tweener.Hide(true);
				else
					messageBox.gameObject.SetActive(false);
			}

			/// <summary>
			/// Sets the message
			/// </summary>
			/// <param name="messageType">The type of the message to send</param>
			/// <param name="itemName">The name of the item</param>
			/// <param name="playerName">The name of the involved player</param>
			/// <returns>The message</returns>
			private string GetMessage(MessageType messageType, string itemName, string playerName)
			{
				string message = "You shouldn't be seeing this... please report this!";

				switch (messageType)
				{
					case MessageType.ReceivedFromSelf:
						message = $"You found your own {itemName}!";
						break;
					case MessageType.ReceivedFromSomeone:
						message = $"{playerName} found your {itemName}!";
						break;
					case MessageType.Sent:
						message = $"You found {itemName} for {playerName}!";
						break;
				}

				return message;
			}

			/// <summary>
			/// Sets the icon texture
			/// </summary>
			private void SetIconTexture()
			{
				Texture2D texture = Resources.Load(IconPath) as Texture2D;

				if (messageBox.texture != texture)
					Resources.UnloadAsset(messageBox.texture);

				messageBox.texture = texture;
				messageBox.mat.mainTexture = texture; ;
			}

			/// <summary>
			/// Colorizes some parts of the message text, such as item or player names
			/// </summary>
			private IEnumerator SetTextColors()
			{
				// Arbitrary delay due to issues with setting message box text property
				yield return new WaitForEndOfFrame();

				// Remove spaces from message so color indices don't get thrown off by them, as
				// Each space has one index for color
				string messageWithoutSpaces = Message.Replace(" ", "");
				string itemNameWithoutSpaces = ItemName.Replace(" ", "");
				Color[] meshColors = messageBox._text.mesh.colors;
				int startItemNameIndex = messageWithoutSpaces.IndexOf(itemNameWithoutSpaces);

				// Multiply index by 4 as each character has 4 color indices
				ReplaceMeshColors(meshColors, startItemNameIndex * 4, itemNameWithoutSpaces.Length * 4, itemNameProgressiveColor);

				// If a player's name is in message
				if (Message.Contains(PlayerName))
				{
					// Remove spaces from message so color indices don't get thrown off by them, as
					// Each space has one index for color
					string playerNameWithoutSpaces = PlayerName.Replace(" ", "");
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
				{
					colors[i] = color;
				}
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
	}
}