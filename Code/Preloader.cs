using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
		public static bool IsPreloading
		{
			get
			{
				return !Instance.objectsToPreload.Keys.ToList().Contains(SceneManager.GetActiveScene().name);
			}
		}

		public Preloader()
		{
			instance = this;
			objectsToPreload = new();
			preloadedObjects = new();
			objectHolder = new GameObject("Preloaded Objects").transform;
			fadeData = ItemRandomizer.Instance.FadeData;
			Object.DontDestroyOnLoad(objectHolder);

			APHandler.Instance.OnDisconnect += () =>
			{
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

		public void StartPreload(OnPreloadDone onPreloadDone = null)
		{
			stopwatch = Stopwatch.StartNew();
			Plugin.StartRoutine(PreloadAll(onPreloadDone));
		}

		private IEnumerator PreloadAll(OnPreloadDone onPreloadDone)
		{
			APMenuStuff apMenuStuff = APMenuStuff.Instance;
			float progressPercent;
			bool hasDoneFadeOut = false;
			int loopCount = 0;

			foreach (KeyValuePair<string, List<OnLoadedSceneFunc>> kvp in objectsToPreload)
			{
				loopCount++;
				string sceneToLoad = kvp.Key;

				if (!hasDoneFadeOut)
				{
					OverlayFader.StartFade(fadeData, true, delegate ()
					{
						apMenuStuff.ToggleLoadingBar(true);
						hasDoneFadeOut = true;
					}, Vector3.zero);
				}

				yield return new WaitUntil(() => { return hasDoneFadeOut; });
				yield return SceneManager.LoadSceneAsync(sceneToLoad);

				PreloadObjects(kvp.Value);

				progressPercent = (float)loopCount / objectsToPreload.Count * 100;
				apMenuStuff.UpdateLoadingBar(progressPercent);
			}

			apMenuStuff.ToggleLoadingBar(false);
			PreloadingFinished(onPreloadDone);
		}

		private void PreloadObjects(List<OnLoadedSceneFunc> callbacks)
		{
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
		}

		private void PreloadingFinished(OnPreloadDone onPreloadDone)
		{
			// Load into saved scene
			IDataSaver startSaver = ModCore.Plugin.MainSaver.GetSaver("/local/start");
			savedScene = startSaver.LoadData("level");
			string sceneToLoad = string.IsNullOrEmpty(savedScene) ? "Intro" : savedScene;
			fadeData._fadeOutTime = 0;
			SceneDoor.StartLoad(sceneToLoad, startSaver.LoadData("door"), fadeData);

			onPreloadDone?.Invoke();
			stopwatch.Stop();
			Plugin.Log.LogInfo($"Finished preloading objects in {stopwatch.ElapsedMilliseconds}ms");
		}

		public delegate Object[] OnLoadedSceneFunc();
		public delegate void OnPreloadDone();
	}
}