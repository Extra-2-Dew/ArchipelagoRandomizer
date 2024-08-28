using System.Collections.Generic;
using System.IO;
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

        private static string bundlePath = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Assets", "apmodels");
        private static List<PreviewItemData> itemData;
        private static Dictionary<string, GameObject> models;
        private static Dictionary<KeyType, Material> keyMaterials;
        private static Dictionary<KeyType, Material> trimMaterials;
        private static Material blackOutline;
        private static Material whiteOutline;

        public static void Reset()
        {
            if (models == null) return;
            foreach (var model in models.Values)
            {
                Object.Destroy(model);
            }
            models = new();
        }

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

        public static GameObject GetModelFromBundle(string itemName)
        {
            Plugin.Log.LogInfo($"Preloading {itemName}...");

            PreviewItemData data = ItemData.FirstOrDefault((x) => x.key == itemName);

            GameObject model = GameObject.Instantiate(ModCore.Utility.LoadAssetFromBundle(bundlePath, data.path));

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
            int type = 0;
            if (key.Contains("Card")) key = "Card";
            if (key.Contains("Key") && key != "Forbidden Key") ChangeKeyColor(key, out type);
            if (!models.ContainsKey(key) && type == 0)
            {
                Plugin.Log.LogError($"Preview Model {key} does not exist.");
                return null;
            }
            if (type == 1) return models["Key"];
            if (type == 2) return models["Secret Key"];
            return models[key];
        }

        private static void ChangeKeyColor(string keyDungeon, out int type)
        {
            if (keyMaterials == null) SetupKeyMaterials();
            KeyType keyType = KeyType.PillowFort;

            if (keyDungeon.Contains("Ring")) keyDungeon = keyDungeon.Replace(" Ring", "");
            Plugin.Log.LogInfo($"Getting key for {keyDungeon}");

            bool secretKey = false;
            bool dreamKey = false;
            type = 0;

            switch (keyDungeon)
            {
                case "Pillow Fort Key":
                    keyType = KeyType.PillowFort; break;
                case "Sand Castle Key":
                    keyType = KeyType.SandCastle; break;
                case "Art Exhibit Key":
                    keyType = KeyType.ArtExhibit; break;
                case "Trash Cave Key":
                    keyType = KeyType.TrashCave; break;
                case "Flooded Basement Key":
                    keyType = KeyType.FloodedBasement; break;
                case "Potassium Mine Key":
                    keyType = KeyType.PotassiumMine; break;
                case "Boiling Grave Key":
                    keyType = KeyType.BoilingGrave; break;
                case "Grand Library Key":
                    keyType = KeyType.GrandLibrary; break;
                case "Sunken Labyrinth Key":
                    keyType = KeyType.SunkenLabyrinth;
                    secretKey = true; break;
                case "Machine Fortress Key":
                    keyType = KeyType.MachineFortress;
                    secretKey = true; break;
                case "Dark Hypostyle Key":
                    keyType = KeyType.DarkHypostyle;
                    secretKey = true; break;
                case "Tomb of Simulacrum Key":
                    keyType = KeyType.TombOfSimulacrum;
                    secretKey = true; break;
                case "Syncope Key":
                    keyType = KeyType.Syncope;
                    dreamKey = true; break;
                case "Antigram Key":
                    keyType = KeyType.Antigram;
                    dreamKey = true; break;
                case "Bottomless Tower Key":
                    keyType = KeyType.BottomlessTower;
                    dreamKey = true; break;
                case "Quietus Key":
                    keyType = KeyType.Quietus;
                    dreamKey = true; break;
            }
            Plugin.Log.LogInfo($"Keytype is {keyType}");

            GameObject key = null;
            if (secretKey)
            {
                key = models["Secret Key"];
                type = 2;
            }
            else
            {
                key = models["Key"];
                type = 1;
            }
            MeshRenderer rend = key.GetComponentInChildren<MeshRenderer>();
            Plugin.Log.LogInfo($"Setting materials for {rend.gameObject.name}, child of {key.name}");
            List<Material> mats = new();
            // set outline color
            if (secretKey)
            {
                mats.Add(keyMaterials[keyType]);
                mats.Add(blackOutline);
                rend.sharedMaterials = mats.ToArray();
                Plugin.Log.LogInfo($"Assigning {keyMaterials[keyType].name} and {blackOutline}");
                Plugin.Log.LogInfo($"Now, the renderer has {rend.sharedMaterials[0].name} and {rend.sharedMaterials[1].name}");
            }
            else
            {
                mats.Add(dreamKey ? whiteOutline : blackOutline);
                mats.Add(keyMaterials[keyType]);
                mats.Add(trimMaterials[keyType]);
                rend.sharedMaterials = mats.ToArray();
                Plugin.Log.LogInfo($"Assigning {keyMaterials[keyType].name}, {trimMaterials[keyType].name}, and {(dreamKey ? whiteOutline.name : blackOutline.name)}");
                Plugin.Log.LogInfo($"Now, the renderer has {rend.sharedMaterials[0].name}, {rend.sharedMaterials[1].name} and {rend.sharedMaterials[2].name}");
            }
        }

        public static void SetupKeyMaterials()
        {
            keyMaterials = new();
            trimMaterials = new();
            GameObject materialsObject = GameObject.Instantiate(ModCore.Utility.LoadAssetFromBundle(bundlePath, "Assets/Extra2Dew/Prefabs/AP/MaterialHolder.prefab"));

            keyMaterials.Add(KeyType.PillowFort, GetMaterial(materialsObject, "Lavender"));
            trimMaterials.Add(KeyType.PillowFort, GetMaterial(materialsObject, "GenericDullPink"));
            keyMaterials.Add(KeyType.SandCastle, GetMaterial(materialsObject, "Tan"));
            trimMaterials.Add(KeyType.SandCastle, GetMaterial(materialsObject, "GenericBrown"));
            keyMaterials.Add(KeyType.ArtExhibit, GetMaterial(materialsObject, "White"));
            trimMaterials.Add(KeyType.ArtExhibit, GetMaterial(materialsObject, "GenericGrey"));
            keyMaterials.Add(KeyType.TrashCave, GetMaterial(materialsObject, "Green"));
            trimMaterials.Add(KeyType.TrashCave, GetMaterial(materialsObject, "GenericGreen"));
            keyMaterials.Add(KeyType.FloodedBasement, GetMaterial(materialsObject, "LightBlue"));
            trimMaterials.Add(KeyType.FloodedBasement, GetMaterial(materialsObject, "GenericBlue"));
            keyMaterials.Add(KeyType.PotassiumMine, GetMaterial(materialsObject, "Brown"));
            trimMaterials.Add(KeyType.PotassiumMine, GetMaterial(materialsObject, "GenericBrown2"));
            keyMaterials.Add(KeyType.BoilingGrave, GetMaterial(materialsObject, "Red"));
            trimMaterials.Add(KeyType.BoilingGrave, GetMaterial(materialsObject, "GenericRed"));
            keyMaterials.Add(KeyType.GrandLibrary, GetMaterial(materialsObject, "Gold"));
            trimMaterials.Add(KeyType.GrandLibrary, GetMaterial(materialsObject, "GenericBrown3"));
            keyMaterials.Add(KeyType.SunkenLabyrinth, GetMaterial(materialsObject, "Purple"));
            keyMaterials.Add(KeyType.MachineFortress, GetMaterial(materialsObject, "Red"));
            keyMaterials.Add(KeyType.DarkHypostyle, GetMaterial(materialsObject, "Blue"));
            keyMaterials.Add(KeyType.TombOfSimulacrum, GetMaterial(materialsObject, "Gold"));
            keyMaterials.Add(KeyType.Syncope, GetMaterial(materialsObject, "Red"));
            trimMaterials.Add(KeyType.Syncope, GetMaterial(materialsObject, "GenericRed"));
            keyMaterials.Add(KeyType.Antigram, GetMaterial(materialsObject, "Lavender"));
            trimMaterials.Add(KeyType.Antigram, GetMaterial(materialsObject, "GenericDullPink"));
            keyMaterials.Add(KeyType.BottomlessTower, GetMaterial(materialsObject, "Blue"));
            trimMaterials.Add(KeyType.BottomlessTower, GetMaterial(materialsObject, "GenericBlue"));
            keyMaterials.Add(KeyType.Quietus, GetMaterial(materialsObject, "Purple"));
            trimMaterials.Add(KeyType.Quietus, GetMaterial(materialsObject, "GenericDullPink"));

            blackOutline = GetMaterial(materialsObject, "OutlineBlack");
            whiteOutline = GetMaterial(materialsObject, "OutlineWhite");
            Object.DontDestroyOnLoad(materialsObject);
            materialsObject.SetActive(false);
        }

        private static Material GetMaterial(GameObject materialsObject, string objectName)
        {
            return materialsObject.transform.Find(objectName).GetComponent<MeshRenderer>().sharedMaterial;
        }

        private enum KeyType
        {
            PillowFort,
            SandCastle,
            ArtExhibit,
            TrashCave,
            FloodedBasement,
            PotassiumMine,
            BoilingGrave,
            GrandLibrary,
            SunkenLabyrinth,
            MachineFortress,
            DarkHypostyle,
            TombOfSimulacrum,
            Syncope,
            Antigram,
            BottomlessTower,
            Quietus
        }
    }
}
