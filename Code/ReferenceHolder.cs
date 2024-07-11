using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	internal class ReferenceHolder
	{
		public SoundClip heartSound;
		private readonly FadeEffectData fadeData;
		private string sceneToLoadWhenDone = "Intro";
		private string spawnToLoadWhenDone;
		private bool hasStoredRefs;
		private Stopwatch stopwatch;

		public List<StatusType> StatusBuffs { get; } = new();
		public List<StatusType> StatusDebuffs { get; } = new();
		public GameObject BeeSwarmSpawner { get; private set; }

		public ReferenceHolder()
		{
			Events.OnFileStart += OnFileStart;
			Events.OnPlayerSpawn += OnPlayerSpawn;
			Events.OnSceneLoaded += OnSceneLoad;

			fadeData = new()
			{
				_targetColor = Color.black,
				_fadeOutTime = 0.5f,
				_fadeInTime = 1.25f,
				_faderName = "ScreenCircleWipe",
				_useScreenPos = true
			};
		}

		private void OnPlayerSpawn(Entity player, GameObject camera, PlayerController controller)
		{
			if (StatusBuffs.Count > 0 && StatusDebuffs.Count > 0)
				return;

			EntityStatusable statusable = EntityTag.GetEntityByName("PlayerEnt").GetEntityComponent<EntityStatusable>();

			foreach (StatusType status in statusable._saveable)
			{
				// These are not to be used currently
				if (status.name.EndsWith("Courage") || status.name.EndsWith("Fortune") || status.name.EndsWith("Curse"))
					continue;

				if (status.name.EndsWith("Fragile") || status.name.EndsWith("Weak"))
					StatusDebuffs.Add(status);
				else
					StatusBuffs.Add(status);

				Object.DontDestroyOnLoad(status);
			}

			Events.OnPlayerSpawn -= OnPlayerSpawn;
		}

		private void OnFileStart(bool newFile)
		{
			if (!newFile)
			{
				IDataSaver saver = ModCore.Plugin.MainSaver.GetSaver("/local/start");
				sceneToLoadWhenDone = saver.LoadData("level");
				spawnToLoadWhenDone = saver.LoadData("door");
			}

			OverlayFader.StartFade(fadeData, true, delegate ()
			{
				stopwatch = Stopwatch.StartNew();
				// Load Former Colossus for Cold status reference
				ModCore.Utility.LoadScene("Deep7");
			}, Vector3.zero);

			Events.OnFileStart -= OnFileStart;
		}

		private void OnSceneLoad(Scene scene, LoadSceneMode mode)
		{
			if (hasStoredRefs)
			{
				DoneStoringReferences();
				return;
			}

			// Former Colossus
			if (scene.name == "Deep7")
			{
				StoreStatus("Cold");
				ModCore.Utility.LoadScene("MachineFortress");
			}
			else if (scene.name == "MachineFortress")
			{
				StoreStatus("Fear");
				BeeSwarmSpawner = GameObject.Find("LevelRoot").transform.Find("O").Find("Doodads").Find("Dungeon_ChestBees").gameObject;
				BeeSwarmSpawner.transform.parent = null;
				Object.DontDestroyOnLoad(BeeSwarmSpawner);
				hasStoredRefs = true;
				fadeData._fadeOutTime = 0;
				SceneDoor.StartLoad(sceneToLoadWhenDone, spawnToLoadWhenDone, fadeData);
			}
		}

		private void StoreStatus(string name)
		{
			StatusType status = Resources.FindObjectsOfTypeAll<StatusType>().FirstOrDefault(status => status.name.EndsWith(name));

			if (status != null)
			{
				StatusDebuffs.Add(status);
				Object.DontDestroyOnLoad(status);
			}
		}

		private void DoneStoringReferences()
		{
			Events.OnSceneLoaded -= OnSceneLoad;
			stopwatch.Stop();

			Plugin.Log.LogInfo($"Took {stopwatch.ElapsedMilliseconds}ms to store references");
		}
	}
}