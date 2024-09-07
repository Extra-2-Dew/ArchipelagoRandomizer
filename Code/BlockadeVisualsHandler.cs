using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public static class BlockadeVisualsHandler
    {
        public static GameObject bcm;
        public static GameObject poofEffect;

        private static List<BlockadeData> bcmData;
        private static Dictionary<string, List<GameObject>> currentSceneBlockades;

        public static void Init()
        {
            string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "blockadeData.json");
            if (!ModCore.Utility.TryParseJson<List<BlockadeData>>(path, out bcmData))
            {
                Plugin.Log.LogError("Unable to load Blockade Data! Blockades will not be available.");
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


                    GameObject blockade = null;
                    if (spawn.isBCM)
                    {
                        blockade = GameObject.Instantiate(bcm);
                    }
                    GameObject room = GameObject.Find("LevelRoot").transform.Find(spawn.room.ToUpper()).gameObject;
                    blockade.transform.position = spawn.position;
                    blockade.transform.SetParent(room.transform, true);
                    blockade.GetComponent<RoomObject>()._room = room.GetComponent<LevelRoom>();
                    Sign sign = blockade.GetComponentInChildren<Sign>();
                    sign._text = spawn.dialogue;
                    sign._reverseTarget = spawn.ittleTalk;
                    if (!currentSceneBlockades.ContainsKey(blockadeName)) currentSceneBlockades.Add(blockadeName, new());
                    currentSceneBlockades[blockadeName].Add(blockade);
                    blockade.SetActive(true);
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
