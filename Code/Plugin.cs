using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using ModCore;

namespace ArchipelagoRandomizer
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("ModCore")]
	public class Plugin : BaseUnityPlugin
	{
		internal static ManualLogSource Log { get; private set; }

		private void Awake()
		{
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
			DebugMenuManager.AddCommands();
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		}

		private void Start()
		{
			DebugMenuManager.Instance.OnDebugMenuInitialized += () => DebugMenuManager.LogToConsole("To connect to an Archipelago server, use 'ap /connect {server:port} {slot} {password}");
		}
	}
}