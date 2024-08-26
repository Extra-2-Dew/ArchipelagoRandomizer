using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class DoorHandler
	{
		private static List<DoorData.Door> doorsData;

		public enum DoorType
		{
			Cave,
			RegionConnection,
		}

		public DoorHandler()
		{
			if (doorsData == null)
			{
				// Parse item JSON
				string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "doorData.json");

				if (!ModCore.Utility.TryParseJson(path, out DoorData? data))
				{
					Plugin.Log.LogError($"DoorHandler failed to deserialize door data JSON and will do nothing!");
					return;
				}

				doorsData = data?.Doors;
			}

			if (RandomizerSettings.Instance.BlockRegionConnections)
				Events.OnSceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
		{
			List<SceneDoor> doorsInScene = new(Resources.FindObjectsOfTypeAll<SceneDoor>());

			foreach (DoorData.Door door in doorsData)
			{
				if (scene.name != door.SceneName)
					continue;

				SceneDoor doorToModify = doorsInScene.Find(x => x.name == door.DoorName);
				ModifiedEntrance modifiedEntrance = doorToModify.gameObject.AddComponent<ModifiedEntrance>();
				modifiedEntrance.SetDoorData(door);
				modifiedEntrance.enabled = true;
			}
		}

		public struct DoorData
		{
			public List<Door> Doors { get; set; }

			public class Door
			{
				[JsonProperty("door")]
				public string DoorName { get; set; }
				[JsonProperty("scene")]
				public string SceneName { get; set; }
				[JsonProperty("type")]
				public DoorType DoorType { get; set; }
				public string EnableFlag { get; set; }
			}
		}
	}
}