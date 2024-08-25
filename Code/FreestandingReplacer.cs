using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    /// <summary>
    /// Utility class for getting and storing information related to items matching contents
    /// </summary>
    public class FreestandingReplacer
    {
        private static List<PreviewItemData> itemData;
        private static Dictionary<string, GameObject> models;

        public static GameObject GetGameObjectFromItem(SpawnItemEventObserver observer)
        {
            return observer._itemPrefab.gameObject;
        }

        public static GameObject GetGameObjectFromSelector(SpawnItemEventObserver observer, int index = 0)
        {
            ItemSelector selector = (ItemSelector)observer._itemSelector;
            Item item = (Item)selector._data[index].result;
            return item.gameObject;
        }

        /// <summary>
        /// Finds an object to use for previewing from the given path. Automatically adjusts for ItemSelectors.
        /// </summary>
        /// <param name="itemName">The item's key in previewItemData.json</param>
        /// <returns></returns>
        public static GameObject GetModelForPreview(string itemName)
        {
            if (itemData == null)
            {
                string dataPath = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "previewItemData.json");
                if (!ModCore.Utility.TryParseJson<List<PreviewItemData>>(dataPath, out itemData))
                {
                    Plugin.Log.LogError("Unable to load preview item data!");
                    return null;
                }
            }

            PreviewItemData data = itemData.FirstOrDefault((x) => x.key == itemName);

            SpawnItemEventObserver observer = GameObject.Find("LevelRoot").transform.Find(data.path).GetComponent<SpawnItemEventObserver>();
            GameObject model = null;
            if (observer._itemSelector == null)
            {
                model = GameObject.Instantiate(GetGameObjectFromItem(observer));
            }
            else
            {
                model = GameObject.Instantiate(GetGameObjectFromSelector(observer, data.index));
            }
            Object.Destroy(model.GetComponent<VarUpdatingItem>());
            Transform child = model.transform.GetChild(0);
            child.position += data.position;
            child.eulerAngles = data.rotation;
            child.localScale = new Vector3(data.scale, data.scale, data.scale);

            model.name = "Preview" + itemName;
            AddModelPreview(itemName, model);
            return model;
        }

        public static void AddModelPreview(string key, GameObject model)
        {
            if (models == null) models = new();
            models.Add(key, model);
        }

        public static GameObject GetModelPreview(string key)
        {
            if (!models.ContainsKey(key))
            {
                Plugin.Log.LogError($"Preview Model {key} does not exist.");
                return null;
            }
            return models[key];
        }
    }
}
