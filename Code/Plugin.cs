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
		internal static bool TestingLocally { get; } = false;

		private APHandler apHandler;
		private ItemRandomizer itemRandomizer;
		private APCommand apCommandHandler;
		private CustomTextHandler customTextHandler;

		private void Awake()
		{
			Instance = this;
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			apHandler = new APHandler();
			apCommandHandler = new APCommand();
			apCommandHandler.AddCommands();
			customTextHandler = new CustomTextHandler();
			DebugMenuManager.LogToConsole("To connect to an ArchipelagoHandler server, use 'ap /connect {server:port} {slot} {password}");

			Events.OnFileStart += (bool newFile) =>
			{
				itemRandomizer = new GameObject("ItemRandomizer").AddComponent<ItemRandomizer>();
				itemRandomizer.OnFileStart(newFile);
			};

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		}

		public static Coroutine StartRoutine(IEnumerator coroutine)
		{
			return Instance.StartCoroutine(coroutine);
		}
	}
}