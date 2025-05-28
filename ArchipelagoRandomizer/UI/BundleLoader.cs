using UnityEngine;

namespace ID2.ArchipelagoRandomizer.UI;

public static class BundleLoader
{
	private static AssetBundle bundle;

	/// <summary>
	/// Returns the loaded asset from the asset bundle.
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="objName">The name of the object</param>
	public static T LoadAssetFromBundle<T>(string objName) where T : UnityEngine.Object
	{
		if (bundle == null)
		{
			LoadBundle();
		}

		if (bundle == null)
		{
			Logger.LogError("Bundle was not loaded. Does the file exist?");
			return null;
		}

		return bundle.LoadAsset<T>(objName);
	}

	private static void LoadBundle()
	{
		bundle = AssetBundle.LoadFromFile(
			BepInEx.Utility.CombinePaths(
				BepInEx.Paths.PluginPath,
				PluginInfo.PLUGIN_NAME,
				"Assets",
				"apmenus"));
	}
}