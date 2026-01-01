using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace ID2.ArchipelagoRandomizer;

[BepInPlugin(PluginInfo.guid, PluginInfo.name, PluginInfo.version)]
public class Plugin : BaseUnityPlugin
{
	internal static new ManualLogSource Logger;
	private static Plugin instance;

	public static Plugin Instance => instance;

	private void Awake()
	{
		instance = this;
		Logger = base.Logger;

		try
		{
			// Mod initialization code here

			Harmony harmony = new(PluginInfo.guid);
			harmony.PatchAll();
		}
		catch (System.Exception ex)
		{
			ArchipelagoRandomizer.Logger.LogError($"Unhandled exception during initialization: {ex.Message}");
			return;
		}

		Logger.LogInfo($"Initialized [{PluginInfo.name} {PluginInfo.version}]");
	}

	/// <summary>
	/// Starts a Coroutine on the Plugin MonoBehaviour.<br/>
	/// This is useful for if you need to start a Coroutine<br/>from a non-MonoBehaviour class.
	/// </summary>
	/// <param name="routine">The routine to start.</param>
	/// <returns>The started Coroutine.</returns>
	public static Coroutine StartRoutine(IEnumerator routine)
	{
		return Instance.StartCoroutine(routine);
	}
}