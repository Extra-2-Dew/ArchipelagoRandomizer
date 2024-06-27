using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModCore;
using System.Reflection;

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

			Events.OnFileStart += (newFile) =>
			{
				new APHandler();
				new APCommand();
				new ItemRandomizer(newFile);
				Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
				DebugMenuManager.LogToConsole("To connect to an ArchipelagoHandler server, use 'ap /connect {server:port} {slot} {password}");
			};
		}
	}
}