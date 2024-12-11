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
		private readonly FadeEffectData fadeData;
		private Stopwatch stopwatch;
		private string savedScene;

		public static Preloader Instance { get { return instance; } }
		public static bool NeedsPreloading
		{
			get
			{
				return Instance.objectsToPreload != null && Instance.objectsToPreload.Count > 0;
			}
		}
		public static bool IsPreloading { get; private set; }

		public Preloader()
		{
			instance = this;
			objectsToPreload = new();
			preloadedObjects = new();
			objectHolder = new GameObject("Preloaded Objects").transform;
			objectHolder.gameObject.SetActive(false);
			fadeData = ItemRandomizer.Instance.FadeData;
			Object.DontDestroyOnLoad(objectHolder);
			APHandler.Instance.OnDisconnect += OnDisconnected;
		}

		/// <summary>
		/// Returns a preloaded object
		/// </summary>
		/// <typeparam name="T">The Type of the object</typeparam>
		/// <param name="name">The name of the object</param>
		/// <param name="keepInactive">Should the object remain inactive? Objects are set to inactive when preloaded.
		/// If this is false, the object will be activated when returned here, so you don't have to do it manually</param>
		public static T GetPreloadedObject<T>(string name) where T : Object
		{
			// Find the preloaded object
			T obj = (T)Instance.preloadedObjects.Find(x => x.name == name);

			if (obj == null)
			{
				Plugin.Log.LogError($"No object with name {name} was found in preload list!");
				return null;
			}

			return obj;
		}

		public static List<Object> GetAllPreloadedObjects()
		{
			return Instance.preloadedObjects;
		}

		public void AddObjectToPreloadList(string scene, OnLoadedSceneFunc onLoadedScene)
		{
			if (objectsToPreload.ContainsKey(scene))
				objectsToPreload[scene].Add(onLoadedScene);
			else
				objectsToPreload.Add(scene, new List<OnLoadedSceneFunc>() { onLoadedScene });
		}

		public void StartPreload(OnPreloadDone onPreloadDone = null)
		{
			Plugin.StartRoutine(PreloadAll(onPreloadDone));
		}

		private IEnumerator PreloadAll(OnPreloadDone onPreloadDone)
		{
			APMenuStuff apMenuStuff = APMenuStuff.Instance;
			float progressPercent;
			bool hasDoneFadeOut = false;
			int loopCount = 0;
			IsPreloading = true;

			foreach (KeyValuePair<string, List<OnLoadedSceneFunc>> kvp in objectsToPreload)
			{
				loopCount++;
				string sceneToLoad = kvp.Key;

				if (!hasDoneFadeOut)
				{
					// Does fadeout after clicking start game
					OverlayFader.StartFade(fadeData, true, delegate ()
					{
						// Fadeout has finished
						stopwatch = Stopwatch.StartNew();
						apMenuStuff.ToggleLoadingBar(true);
						hasDoneFadeOut = true;
					}, Vector3.zero);
				}

				// Waits for fadeout to finish
				yield return new WaitUntil(() => { return hasDoneFadeOut; });
				// Wait for scene to load
				yield return SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
				Plugin.Log.LogInfo($"Preloading scene {sceneToLoad}...");
				SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));

				PreloadObjects(kvp.Value);

				progressPercent = (float)loopCount / objectsToPreload.Count * 100;
				apMenuStuff.UpdateLoadingBar(progressPercent);

				// Done with scene, so unload it
				yield return SceneManager.UnloadSceneAsync(sceneToLoad);
			}

			apMenuStuff.ToggleLoadingBar(false);
			PreloadingFinished(onPreloadDone);
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

		private void PreloadingFinished(OnPreloadDone onPreloadDone)
		{
			stopwatch.Stop();
			Plugin.Log.LogInfo($"Finished preloading {preloadedObjects.Count} object(s) across {objectsToPreload.Count} scene(s) in {stopwatch.ElapsedMilliseconds}ms");

			IsPreloading = false;
			objectsToPreload.Clear();
			onPreloadDone?.Invoke();

			// Load into saved scene
			IDataSaver startSaver = ModCore.Plugin.MainSaver.GetSaver("/local/start");
			savedScene = startSaver.LoadData("level");
			string sceneToLoad = string.IsNullOrEmpty(savedScene) ? "Intro" : savedScene;
			fadeData._fadeOutTime = 0;
			SceneDoor.StartLoad(sceneToLoad, startSaver.LoadData("door"), fadeData);
		}

		private void OnDisconnected()
		{
			Object.Destroy(objectHolder.gameObject);
			APHandler.Instance.OnDisconnect -= OnDisconnected;
		}

		public delegate Object[] OnLoadedSceneFunc();
		public delegate void OnPreloadDone();
	}
}