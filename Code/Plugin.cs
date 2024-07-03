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

		public static Coroutine StartRoutine(IEnumerator coroutine)
		{
			return Instance.StartCoroutine(coroutine);
		}
	}
}