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
        private static List<PreviewItemData> ItemData
        {
            get
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
                return itemData;
            }
            set
            {
                itemData = value;
            }
        }

        private static List<PreviewItemData> itemData;
        private static Dictionary<string, GameObject> models;

        private static GameObject GetGameObjectFromItem(SpawnItemEventObserver observer)
        {
            return observer._itemPrefab.gameObject;
        }

        private static GameObject GetGameObjectFromSelector(SpawnItemEventObserver observer, int index = 0)
        {
            ItemSelector selector = (ItemSelector)observer._itemSelector;
            Item item;
            if (selector._data.Length > 0 && index > -1)
            {
                item = (Item)selector._data[index].result;
            }
            else
            {
                item = (Item)selector._fallback;
            }
            return item.gameObject;
        }

        /// <summary>
        /// Finds an object to use for previewing from the given path. Automatically adjusts for ItemSelectors.
        /// </summary>
        /// <param name="itemName">The item's key in previewItemData.json</param>
        /// <returns></returns>
        public static GameObject GetModelFromPath(string itemName)
        {
            Plugin.Log.LogInfo($"Preloading {itemName}...");

            PreviewItemData data = ItemData.FirstOrDefault((x) => x.key == itemName);

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

            return GenerateModelPrefab(model, data, itemName);
        }

        public static GameObject GetModelFromDroptable(string itemName)
        {
            Plugin.Log.LogInfo($"Preloading {itemName}...");

            PreviewItemData data = ItemData.FirstOrDefault((x) => x.key == itemName);

            DropTable[] dropTables = Resources.FindObjectsOfTypeAll<DropTable>();
            DropTable table = null;
            foreach (DropTable dropTable in dropTables )
            {
                if (dropTable.name == data.path)
                {
                    table = dropTable;
                    break;
                }
            }

            GameObject model = GameObject.Instantiate(table._items[data.index].gameObject);
            Object.Destroy(model.GetComponent<StatusClearingItem>());
            Object.Destroy(model.GetComponent<HealthAffectingItem>());
            Object.Destroy(model.GetComponent<PickupEffectItem>());
            Object.Destroy(model.GetComponent<RandomStatusItem>());
            Object.Destroy(model.GetComponent<BC_ColliderSphere>());
            Object.Destroy(model.GetComponent<GameActionItem>());
            Object.Destroy(model.GetComponent<GA_Explode>());
            Object.Destroy(model.GetComponent<EnvirodeathableItem>());
            Object.Destroy(model.GetComponent<Envirodeathable>());
            Object.Destroy(model.GetComponent<Item>());
            Object.Destroy(model.GetComponentInChildren<Animation>());
            Object.Destroy(model.GetComponentInChildren<AnimatingItem>());
            Object.Destroy(model.GetComponentInChildren<EntityAnimator>());

            // Lightning particle fix
            if (itemName == "Lightning")
            {
                model.transform.Find("Particle System").SetParent(model.transform.Find("crystal"));
            }

            return GenerateModelPrefab(model, data, itemName);
        }

        public static GameObject GetModelFromGameObject(string itemName)
        {
            Plugin.Log.LogInfo($"Preloading {itemName}...");

            PreviewItemData data = ItemData.FirstOrDefault((x) => x.key == itemName);

            GameObject model = GameObject.Instantiate(GameObject.Find("LevelRoot").transform.Find(data.path).gameObject);
            Object.Destroy(model.GetComponent<Animator>());
            return GenerateModelPrefab(model, data, itemName);
        }

        public static GameObject GetModelFromSpawner(string itemName)
        {
            Plugin.Log.LogInfo($"Preloading {itemName}...");

            PreviewItemData data = ItemData.FirstOrDefault((x) => x.key == itemName);

            GameObject model = GameObject.Instantiate(ItemRandomizer.GetEntityFromSpawner(data.path).gameObject);


            Object.Destroy(model.GetComponent<GameStatEntityDeath>());
            Object.Destroy(model.GetComponent<BC_ColliderCylinder8>());
            Object.Destroy(model.GetComponent<Moveable>());
            Object.Destroy(model.GetComponent<BC_Body>());
            Object.Destroy(model.GetComponent<EntityStatusable>());
            Object.Destroy(model.GetComponent<EntityAnimator>());
            Object.Destroy(model.GetComponent<RigidBodyController>());
            Object.Destroy(model.GetComponent<Entity>());
            Object.Destroy(model.GetComponent<Hittable>());
            Object.Destroy(model.GetComponent<EntityHittable>());
            Object.Destroy(model.transform.Find("Killable").gameObject);
            Object.Destroy(model.transform.Find("Attacks").gameObject);

            return GenerateModelPrefab(model, data, itemName);
        }

        public static GameObject GenerateModelPrefab(GameObject model, PreviewItemData data, string itemName)
        {
            Object.Destroy(model.GetComponent<VarUpdatingItem>());
            GameObject appearanceApplier = null;
            if (data.child > -1)
            {
                appearanceApplier = model.transform.GetChild(data.child).gameObject;
            }
            else
            {
                appearanceApplier = model;
            }
            appearanceApplier.transform.localPosition = data.position;
            appearanceApplier.transform.localEulerAngles = data.rotation;
            appearanceApplier.transform.localScale = new Vector3(data.scale, data.scale, data.scale);

            model.name = "Preview " + itemName;

            if (data.spin)
            {
                appearanceApplier.gameObject.AddComponent<SpinAnimation>();
            }

            AddModelPreview(itemName, model);
            if (!string.IsNullOrEmpty(data.copyTo))
            {
                AddModelPreview(data.copyTo, model);
            }

            return model;
        }

        public static void AddModelPreview(string key, GameObject model)
        {
            if (models == null) models = new();
            models.Add(key, model);
        }

        public static GameObject GetModelPreview(string key)
        {
            if (key.Contains("Card")) key = "Card";
            if (key.Contains("Key") && key != "Forbidden Key") key = "Key";
            // TODO change to key-specific method
            if (!models.ContainsKey(key))
            {
                Plugin.Log.LogError($"Preview Model {key} does not exist.");
                return null;
            }
            return models[key];
        }
    }
}
