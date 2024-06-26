using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
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

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		}
	}
}