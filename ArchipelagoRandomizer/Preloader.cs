using HarmonyLib;
using ID2.ArchipelagoRandomizer.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ID2.ArchipelagoRandomizer;

class Preloader
{
	private readonly static Preloader instance = new();
	private readonly Transform objectHolder;
	private readonly Dictionary<string, List<OnLoadedSceneFunc>> objectsToPreload;
	private readonly List<Object> preloadedObjects;
	private readonly FadeEffectData fadeData;
	private Stopwatch stopwatch;

	public static Preloader Instance => instance;
	public static bool IsPreloading { get; private set; }

	private Preloader()
	{
		// Create object holder object
		objectHolder = new GameObject("Preloaded Objects").transform;
		objectHolder.gameObject.SetActive(false);

		// Create fade data
		fadeData = ModCore.Utility.MakeFadeData(
			Color.black,
			fadeOutTime: 0.5f,
			fadeInTime: 1.25f,
			fadeType: ModCore.FadeType.ScreenCircleWipe,
			useScreenPos: true
		);

		// Initialize collections
		objectsToPreload = [];
		preloadedObjects = [];

		// Make object holder object persistent
		Object.DontDestroyOnLoad(objectHolder);

		Events.OnSceneLoaded += OnSceneLoaded;
	}

	public static T GetPreloadedObject<T>(string objName) where T : Object
	{
		// Find the preloaded object
		T obj = (T)Instance.preloadedObjects.Find(x => x.name == objName);

		if (obj == null)
		{
			Logger.LogError($"No object with name '{objName}' was found in preload list!");
			return null;
		}

		return obj;
	}

	public void AddObjectToPreloadList(string scene, OnLoadedSceneFunc onLoadedSceneCallback)
	{
		// If the scene is already in preload list, add the new callback to it
		if (objectsToPreload.ContainsKey(scene))
		{
			if (objectsToPreload[scene].Contains(onLoadedSceneCallback))
			{
				return;
			}

			objectsToPreload[scene].Add(onLoadedSceneCallback);
		}
		// If the scene is not already in preload list, add the new scene
		else
		{
			objectsToPreload.Add(scene, [onLoadedSceneCallback]);
		}
	}

	public void StartPreload(Action onDone = null)
	{
		IsPreloading = true;
		Logger.Log("Starting preload...");
		CoroutineRunner.Start(PreloadAll(onDone));
	}

	private IEnumerator PreloadAll(Action onDone)
	{
		PreloadingScreen preloadingScreen = new();
		float progressPercent;
		bool hasDoneFadeOut = false;
		int loopCount = 0;

		foreach (KeyValuePair<string, List<OnLoadedSceneFunc>> kvp in objectsToPreload)
		{
			loopCount++;
			string sceneToLoad = kvp.Key;

			// Does fadeout after clicking start game
			if (!hasDoneFadeOut)
			{
				OverlayFader.StartFade(fadeData, true, delegate ()
				{
					stopwatch = Stopwatch.StartNew();
					preloadingScreen.TogglePreloadingScreen(true);
					hasDoneFadeOut = true;
				}, Vector3.zero);
			}

			// Wait for fadeout to finish
			yield return new WaitUntil(() => { return hasDoneFadeOut; });

			// Wait for scene to load
			yield return SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
			Logger.Log($"Preloading scene {sceneToLoad}...");
			Scene savedScene = SceneManager.GetActiveScene();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
			yield return SceneManager.UnloadSceneAsync(savedScene);

			PreloadObjects(kvp.Value);

			// Update progress
			progressPercent = (float)loopCount / objectsToPreload.Count * 100;
			preloadingScreen.UpdateProgressBar(progressPercent);

		}

		preloadingScreen.TogglePreloadingScreen(false);
		PreloadFinished(onDone);
	}

	private void PreloadObjects(List<OnLoadedSceneFunc> callbacks)
	{
		for (int i = 0; i < callbacks.Count; i++)
		{
			// Get the objects to preload from the callback
			Object[] objs = callbacks[i]?.Invoke();

			foreach (Object obj in objs)
			{
				GameObject gameObj = obj as GameObject;

				// If Object is a GameObject, change its parent so it persists
				if (gameObj != null)
					gameObj.transform.SetParent(objectHolder, true);
				else
					Object.DontDestroyOnLoad(obj);

				preloadedObjects.Add(obj);
			}
		}
	}

	private void PreloadFinished(Action onDone)
	{
		stopwatch.Stop();
		Logger.Log($"Finished preloading {preloadedObjects.Count} object(s) across {objectsToPreload.Count} scene(s) in {stopwatch.ElapsedMilliseconds}ms");

		IsPreloading = false;
		objectsToPreload.Clear();
		onDone?.Invoke();

		// Load into saved scene
		IDataSaver startSaver = ModCore.Plugin.MainSaver.GetSaver("/local/start");
		string savedScene = startSaver.LoadData("level");
		string sceneToLoad = string.IsNullOrEmpty(savedScene) ? "Intro" : savedScene;
		fadeData._fadeOutTime = 0;
		SceneDoor.StartLoad(sceneToLoad, startSaver.LoadData("door"), fadeData);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenu")
		{
			preloadedObjects.Clear();
			objectsToPreload.Clear();
		}
	}

	public delegate Object[] OnLoadedSceneFunc();

	[HarmonyPatch]
	private class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(ChangeRespawnerEventObserver), nameof(ChangeRespawnerEventObserver.DoChange))]
		private static bool PreventRemedyCheckpointFromSavingPatch()
		{
			return !IsPreloading;
		}
	}
}