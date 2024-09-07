﻿using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public static class BlockadeVisualsHandler
    {
        public static GameObject bcm;
        public static GameObject poofEffect;

        private static List<BCMData> bcmData;
        private static Dictionary<string, List<GameObject>> currentSceneBlockades;

        public static void Init()
        {
            string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "bcmData.json");
            if (!ModCore.Utility.TryParseJson<List<BCMData>>(path, out bcmData))
            {
                Plugin.Log.LogError("Unable to load BCM Data! BCM blockades will not be available.");
                return;
            }
            bcm.GetComponentInChildren<Sign>()._configString = null;

            ItemRandomizer.OnItemReceived += DisableBlockades;
        }

        public static void SpawnBlockades(string sceneName)
        {
            currentSceneBlockades = new();
            var validSpawns = bcmData.FindAll((x) => x.scene == sceneName);
            if (validSpawns != null && validSpawns.Count > 0)
            {
                foreach (var spawn in validSpawns)
                {
                    string blockadeName = $"Connection - {spawn.blockadeName}";

                    if (ItemHandler.GetItemCount(ItemHandler.GetItemData(blockadeName)) > 0) continue;

                    GameObject newBCM = GameObject.Instantiate(bcm);
                    GameObject room = GameObject.Find("LevelRoot").transform.Find(spawn.room.ToUpper()).gameObject;
                    newBCM.transform.position = spawn.position;
                    newBCM.transform.SetParent(room.transform, true);
                    newBCM.GetComponent<RoomObject>()._room = room.GetComponent<LevelRoom>();
                    newBCM.GetComponentInChildren<Sign>()._text = spawn.dialogue;
                    if (!currentSceneBlockades.ContainsKey(blockadeName)) currentSceneBlockades.Add(blockadeName, new());
                    currentSceneBlockades[blockadeName].Add(newBCM);
                    newBCM.SetActive(true);
                }
            }
        }

        public static void DisableBlockades(ItemHandler.ItemData.Item item, string _)
        {
            if (item.ItemName.Contains("Connection - ") && currentSceneBlockades.ContainsKey(item.ItemName))
            {
                foreach (GameObject blockade in currentSceneBlockades[item.ItemName])
                {
                    EffectFactory.Instance.PlayQuickEffect(poofEffect.GetComponent<SimpleQuickParticleEffect>(), blockade.transform.position, blockade);
                    blockade.SetActive(false);
                }
            }
        }

        public class BCMData
        {
            public string scene;
            public string room;
            // should not include "Connection - "
            public string blockadeName;
            public Vector3 position;
            public string dialogue;
        }
    }
}
