using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ID2.ArchipelagoRandomizer;

class Preloader
{
	private readonly static Preloader instance = new();
	private readonly Transform objectHolder;
	private readonly Dictionary<string, List<OnLoadedSceneFunc>> objectsToPreload;
	private readonly List<Object> preloadedObjects;

	public static Preloader Instance => instance;

	private Preloader()
	{
		// Create object holder object
		objectHolder = new GameObject("Preloaded Objects").transform;
		objectHolder.gameObject.SetActive(false);

		// Initialize collections
		objectsToPreload = [];
		preloadedObjects = [];

		// Make object holder object persistent
		Object.DontDestroyOnLoad(objectHolder);
	}

	public static void AddObjectToPreloadList(string scene, OnLoadedSceneFunc onLoadedSceneCallback)
	{
		// If the scene is already in preload list, add the new callback to it
		if (Instance.objectsToPreload.ContainsKey(scene))
		{
			Instance.objectsToPreload[scene].Add(onLoadedSceneCallback);
		}
		// If the scene is not already in preload list, add the new scene
		else
		{
			Instance.objectsToPreload.Add(scene, [onLoadedSceneCallback]);
		}
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

	public void StartPreload()
	{
		CoroutineRunner.Start(PreloadAll(PreloadFinished));
	}

	private IEnumerator PreloadAll(Action onDone)
	{
		yield return new WaitForEndOfFrame();
		onDone?.Invoke();
	}

	private void PreloadFinished()
	{
		Logger.Log("Preloading finished!");
	}

	public delegate Object[] OnLoadedSceneFunc();
}