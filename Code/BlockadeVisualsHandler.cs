using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public static class BlockadeVisualsHandler
	{
		public static GameObject bcm;
		public static GameObject poofEffect;
		public static Dictionary<string, MapData> editedDatas;

		private static GameObject blockadeStand;
		private static string bundlePath = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Assets", "apmodels");
		private static List<BlockadeData> blockadeData;
		private static Dictionary<string, List<GameObject>> currentSceneBlockades;
		private static Texture2D lockedPath;
		private static Texture2D openPath;
		private static MapIconData blockadeIconData;
		private static SaverOwner mainSaver;

		public static void Init()
		{
			editedDatas = new();
			mainSaver = ModCore.Plugin.MainSaver;

			if (!RandomizerSettings.Instance.BlockRegionConnections) return;

			string dataPath = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "blockadeData.json");
			string assetPath = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Assets");
			if (!ModCore.Utility.TryParseJson<List<BlockadeData>>(dataPath, out blockadeData))
			{
				Plugin.Log.LogError("Unable to load Blockade Data! Blockades will not be available.");
				return;
			}
			bcm.GetComponentInChildren<Sign>()._configString = null;

			if (blockadeStand == null)
			{
				blockadeStand = GameObject.Instantiate(ModCore.Utility.LoadAssetFromBundle(bundlePath, "Assets/Extra2Dew/Prefabs/AP/Blockade.prefab"));
				GameObject newSign = GameObject.Instantiate(bcm.GetComponentInChildren<Sign>().gameObject);
				newSign.transform.SetParent(blockadeStand.transform, false);
				Object.DontDestroyOnLoad(blockadeStand);
				blockadeStand.SetActive(false);
			}

			lockedPath = ModCore.Utility.GetTextureFromFile(assetPath + "/MapIconBlockadeClosed.png");
			openPath = ModCore.Utility.GetTextureFromFile(assetPath + "/MapIconBlockadeOpen.png");

			CreateBlockadeIconData();

			CreateMapMarkers();

			ItemRandomizer.OnItemReceived += DisableBlockades;
		}

		private static void CreateBlockadeIconData()
		{
			blockadeIconData = ScriptableObject.CreateInstance<MapIconData>();
			blockadeIconData.name = "BlockadeIconData";
			blockadeIconData._icon = lockedPath;
			blockadeIconData._animator = "big";
			MapMarkerPoint.AltIcon altIcon = new();
			altIcon.name = "cleared";
			altIcon.icon = openPath;
			altIcon.animator = "big";
			blockadeIconData._altIcons = [altIcon];
		}

		public static void CreateMapMarkers()
		{
			IDataSaver markerSaver = mainSaver.GetSaver("/local/markers");

			foreach (BlockadeData blockade in blockadeData)
			{
				ItemHandler.ItemData.Item connectionItem = ItemHandler.GetItemData($"Connection - {blockade.blockadeName}");

				// There is no connection item for Fluffy rooms C -> A or C -> B, so this avoids null ref
				if (connectionItem == null)
				{
					continue;
				}

				string saveFlagCoord = $"{Mathf.Abs(Mathf.CeilToInt(blockade.position.x))}_{Mathf.Abs(Mathf.CeilToInt(blockade.position.z))}";
				string saveFlag = connectionItem.SaveFlag + "_" + saveFlagCoord;
				if (!markerSaver.HasData(saveFlag))
				{
					markerSaver.SaveData(saveFlag, saveFlag);
				}
				if (!editedDatas.ContainsKey(blockade.scene))
				{
					MapData mapData = Resources.Load<MapData>($"Maps/{blockade.scene}");
					MapData newData = ScriptableObject.Instantiate(mapData);
					editedDatas.Add(blockade.scene, newData);
				}
				MapData workingData = editedDatas[blockade.scene];
				List<MapData.MarkerData> markers = workingData.GetMarkers().ToList();
				markers.Add(new(
					blockade.position,
					saveFlag,
					string.Empty,
					blockadeIconData
					));
				workingData._markers = markers.ToArray();
			}

			mainSaver.SaveLocal();
		}

		public static void SpawnBlockades(string sceneName)
		{
			currentSceneBlockades = new();
			var validSpawns = blockadeData.FindAll((x) => x.scene == sceneName);
			if (validSpawns != null && validSpawns.Count > 0)
			{
				foreach (BlockadeData spawn in validSpawns)
				{
					string blockadeName = $"Connection - {spawn.blockadeName}";

					if (ItemHandler.GetItemCount(ItemHandler.GetItemData(blockadeName)) > 0) continue;


					GameObject blockade = null;
					if (spawn.isBCM)
					{
						blockade = GameObject.Instantiate(bcm);
					}
					else
					{
						blockade = GameObject.Instantiate(blockadeStand);
						GameObject blockadeTop = blockade.transform.Find("BlockadeTop").gameObject;
						blockadeTop.transform.localScale = new(0.5f * spawn.width, 0.5f, 0.5f);
						blockade.transform.Find("BlockadeSupportLeft").position = blockadeTop.transform.Find("SupportAnchorLeft").position;
						blockade.transform.Find("BlockadeSupportRight").position = blockadeTop.transform.Find("SupportAnchorRight").position;
						blockade.transform.eulerAngles = new(0, spawn.rotation, 0);
					}
					GameObject room = GameObject.Find("LevelRoot").transform.Find(spawn.room.ToUpper()).gameObject;
					blockade.name = blockadeName;
					blockade.transform.position = spawn.position;
					if (room.transform.Find("Doodads") != null)
					{
						blockade.transform.SetParent(room.transform.Find("Doodads"), true);
					}
					else
					{
						blockade.transform.SetParent(room.transform, true);
					}
					if (blockade.GetComponent<RoomObject>() != null) MonoBehaviour.Destroy(blockade.GetComponent<RoomObject>());
					Sign sign = blockade.GetComponentInChildren<Sign>();
					if (string.IsNullOrEmpty(spawn.dialogue))
					{
						sign.gameObject.SetActive(false);
					}
					else
					{
						sign._text = spawn.dialogue;
						sign._reverseTarget = spawn.ittleTalk;
					}
					if (!currentSceneBlockades.ContainsKey(blockadeName)) currentSceneBlockades.Add(blockadeName, new());
					currentSceneBlockades[blockadeName].Add(blockade);
					blockade.SetActive(true);
				}
			}
		}

		public static void DisableBlockades(ItemHandler.ItemData.Item item, string _)
		{
			if (item.ItemName.Contains("Connection - "))
			{
				if (currentSceneBlockades.ContainsKey(item.ItemName))
				{
					foreach (GameObject blockade in currentSceneBlockades[item.ItemName])
					{
						EffectFactory.Instance.PlayQuickEffect(poofEffect.GetComponent<SimpleQuickParticleEffect>(), blockade.transform.position, blockade);
						blockade.SetActive(false);
					}
				}

				// edit the blockade's map marker to be a checkmark
				string blockadeName = item.ItemName.Replace("Connection - ", "");
				IDataSaver markerSaver = mainSaver.GetSaver("/local/markers");

				foreach (BlockadeData blockade in blockadeData)
				{
					if (blockade.blockadeName != blockadeName) continue;
					string saveFlagCoord = $"{Mathf.Abs(Mathf.CeilToInt(blockade.position.x))}_{Mathf.Abs(Mathf.CeilToInt(blockade.position.z))}";
					string saveFlag = ItemHandler.GetItemData($"Connection - {blockade.blockadeName}").SaveFlag + "_" + saveFlagCoord;
					markerSaver.SaveData(saveFlag, saveFlag + ".cleared");
				}
				mainSaver.SaveLocal();
			}
		}

		public class BlockadeData
		{
			public string scene;
			public string room;
			// should not include "Connection - "
			public string blockadeName;
			public bool isBCM = false;
			public bool ittleTalk = false;
			public Vector3 position;
			// Y axis only
			public float rotation = 0;
			public float width = 1;
			public string dialogue = "";
		}
	}
}
