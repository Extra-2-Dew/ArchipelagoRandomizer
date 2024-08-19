using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	public class Preloader
	{
		private static Preloader instance;
		private readonly Dictionary<string, List<OnLoadedSceneFunc>> objectsToPreload;
		private readonly Transform objectHolder;
		public readonly List<Object> preloadedObjects;
		private Stopwatch stopwatch;
		private string currentScene;

		public static Preloader Instance { get { return instance; } }
		public bool IsPreloading { get; private set; }
		public float PreloadingProgress { get; private set; }

		public Preloader()
		{
			instance = this;
			objectsToPreload = new();
			preloadedObjects = new();
			objectHolder = new GameObject("Preloaded Objects").transform;
			Object.DontDestroyOnLoad(objectHolder);

			Events.OnSceneLoaded += OnSceneLoaded;
			APHandler.Instance.OnDisconnect += () =>
			{
				Events.OnSceneLoaded -= OnSceneLoaded;
				Object.Destroy(objectHolder.gameObject);
			};
		}

		public static T GetPreloadedObject<T>(string name) where T : Object
		{
			return (T)Instance.preloadedObjects.Find(x => x.name == name);
		}

		public void AddObjectToPreloadList(string scene, OnLoadedSceneFunc onLoadedScene)
		{
			if (objectsToPreload.ContainsKey(scene))
				objectsToPreload[scene].Add(onLoadedScene);
			else
				objectsToPreload.Add(scene, new List<OnLoadedSceneFunc>() { onLoadedScene });
		}

		public void StartPreload()
		{
			stopwatch = Stopwatch.StartNew();
			Plugin.StartRoutine(PreloadAll());
		}

		private IEnumerator PreloadAll()
		{
			IsPreloading = true;
			int loopCount = 0;

			foreach (KeyValuePair<string, List<OnLoadedSceneFunc>> kvp in objectsToPreload)
			{
				loopCount++;
				string sceneToLoad = kvp.Key;

				// Wait until that scene has loaded
				yield return SceneManager.LoadSceneAsync(sceneToLoad);

				List<OnLoadedSceneFunc> callbacks = kvp.Value;

				for (int i = 0; i < callbacks.Count; i++)
				{
					Object[] objs = callbacks[i]?.Invoke();

					foreach (Object obj in objs)
					{
						GameObject gameObj = obj as GameObject;

						// If Object is a GameObject, change its parent so it persists
						if (gameObj != null)
							gameObj.transform.parent = objectHolder;
						else
							Object.DontDestroyOnLoad(obj);

						preloadedObjects.Add(obj);
					}
				}

				PreloadingProgress = (float)loopCount / objectsToPreload.Count * 100;
				Plugin.Log.LogInfo($"Preloading progress: {PreloadingProgress}%");
			}

			IsPreloading = false;
			PreloadingFinished();
		}

		private void PreloadingFinished()
		{
			// Load into saved scene
			IDataSaver startSaver = ModCore.Plugin.MainSaver.GetSaver("/local/start");
			string savedScene = startSaver.LoadData("level");
			string sceneToLoad = string.IsNullOrEmpty(savedScene) ? "Intro" : savedScene;
			SceneDoor.StartLoad(sceneToLoad, startSaver.LoadData("door"), ItemRandomizer.Instance.FadeData);

			stopwatch.Stop();
			Plugin.Log.LogInfo($"Finished preloading objects in {stopwatch.ElapsedMilliseconds}ms");
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			currentScene = scene.name;
		}

		public delegate Object[] OnLoadedSceneFunc();
	}
}