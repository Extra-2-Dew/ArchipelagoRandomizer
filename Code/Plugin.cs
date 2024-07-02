using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModCore;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("ModCore")]
	public class Plugin : BaseUnityPlugin
	{
		internal static Plugin Instance { get; private set; }
		internal static ManualLogSource Log { get; private set; }

		private void Awake()
		{
			Instance = this;
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

			Events.OnFileStart += (newFile) =>
			{
				new APHandler();
				new ItemRandomizer(newFile);

				if (ItemRandomizer.Instance.IsActive)
				{
					new APCommand();
					DebugMenuManager.LogToConsole("To connect to an ArchipelagoHandler server, use 'ap /connect {server:port} {slot} {password}");
				}
			};
		}

		public static IEnumerator Test(string message)
        {
			yield return new WaitForEndOfFrame();
			EntityHUD hud = EntityHUD.GetCurrentHUD();
			ItemMessageBox messageBox = EntityHUD.GetCurrentHUD().currentMsgBox;

			if (messageBox != null && messageBox.IsActive)
				messageBox.Hide(true);

            Debug.Log($"Yo you reached me at frame {Time.frameCount}");

			messageBox = OverlayWindow.GetPooledWindow(hud._data.GetItemBox);

			if (messageBox._tweener != null)
				messageBox._tweener.Show(true);
			else
				messageBox.gameObject.SetActive(true);

            Debug.Log($"Yo ho you reached me at frame {Time.frameCount}");
            yield return new WaitForEndOfFrame();
            Debug.Log($"And then you reached me at frame {Time.frameCount}, hud is {hud.name}, message box is {messageBox.name}");
            yield return new WaitForEndOfFrame();
            Debug.Log($"And then you reached me at frame {Time.frameCount}");
            yield return new WaitForEndOfFrame();

            // SEEMS TO IMPROVE IT??
            //messageBox._text.Text = args[0];
            messageBox._text.StringText = new StringHolder.OutString(message);

			messageBox.timer = messageBox._showTime;
			messageBox.countdown = messageBox._showTime > 0;
			Debug.Log($"And now it's {Time.frameCount}");
		}
	}
}