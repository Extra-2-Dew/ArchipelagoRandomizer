using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    /// <summary>
    /// Utility class for getting and storing information related to items matching contents
    /// </summary>
    public class FreestandingReplacer
    {
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
        /// <param name="path">The path to the GameObject that has the SpawnItemEventObserver. Do not include LevelRoot.</param>
        /// <param name="selectorIndex">If the item is part of a selector, which one should be used?</param>
        /// <returns></returns>
        public static GameObject GetModelForPreview(string path, string itemName, int selectorIndex = 0)
        {
            SpawnItemEventObserver observer = GameObject.Find("LevelRoot").transform.Find(path).GetComponent<SpawnItemEventObserver>();
            GameObject model = null;
            if (observer._itemSelector == null)
            {
                model = GameObject.Instantiate(GetGameObjectFromItem(observer));
            }
            else
            {
                model = GameObject.Instantiate(GetGameObjectFromSelector(observer, selectorIndex));
            }
            Object.Destroy(model.GetComponent<VarUpdatingItem>());
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
