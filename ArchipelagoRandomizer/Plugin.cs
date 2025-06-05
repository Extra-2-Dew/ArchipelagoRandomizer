using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ID2.ArchipelagoRandomizer;

[BepInPlugin("id2.archipelagorandomizer", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
//[BepInDependency("id2.modcore")]
[BepInDependency("id2.itemchanger")]
class Plugin : BaseUnityPlugin
{
	internal static new ManualLogSource Logger;
	private ItemRandomizer itemRando;

	private void Awake()
	{
		try
		{
			Logger = base.Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			Events.OnFileStart += (bool newFile) =>
			{
				itemRando = new ItemRandomizer(newFile);
			};

			new Harmony("id2.archipelagorandomizer").PatchAll();
		}
		catch (System.Exception err)
		{
			throw err;
		}
	}
}