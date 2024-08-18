using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModCore;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("ModCore")]
	public class Plugin : BaseUnityPlugin
	{
		private APHandler apHandler;
		private ItemRandomizer itemRandomizer;
		private APCommand apCommandHandler;

		internal static Plugin Instance { get; private set; }
		internal static ManualLogSource Log { get; private set; }
		internal ItemRandomizer.APFileData APFileData { get; private set; }
		internal static bool IsDebug { get; } = true;

		public static void LogDebugMessage(string message)
		{
			if (IsDebug)
			{
				StackFrame stack = new StackTrace().GetFrame(1);
				string callingClassName = stack.GetMethod().DeclaringType?.Name;
				string callingMethodName = stack.GetMethod().Name;
				Log.LogMessage($"[{callingClassName}.{callingMethodName}]\n" + message);
			}
		}

		public static Coroutine StartRoutine(IEnumerator coroutine)
		{
			return Instance.StartCoroutine(coroutine);
		}

		public void SetAPFileData(ItemRandomizer.APFileData apFileData)
		{
			APFileData = apFileData;
		}

		private void Awake()
		{
			Instance = this;
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			apHandler = new APHandler();
			apCommandHandler = new APCommand();
			apCommandHandler.AddCommands();
			new GameObject("MessageBoxHandler").AddComponent<MessageBoxHandler>();
			DebugMenuManager.LogToConsole("To send commands or chat to the Archipelago server, use the \"ap\" command followed by the text you want to send.");

			Events.OnSceneLoaded += (UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) =>
			{
				if (scene.name == "MainMenu")
					new GameObject("APMenuStuff").AddComponent<APMenuStuff>();
			};

			Events.OnFileStart += (bool newFile) =>
			{
				if (APHandler.Instance.IsConnected && APFileData != null)
				{
					itemRandomizer = new GameObject("ItemRandomizer").AddComponent<ItemRandomizer>();
					itemRandomizer.OnFileStart(newFile, APFileData);
				}
			};

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		}
	}
}