using UnityEngine;
using UnityEngine.SceneManagement;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Enums;
using System.Collections.Generic;

namespace ArchipelagoRandomizer
{
    public class PreviewItemInfo : MonoBehaviour
    {
        private GameObject previewObject;
        private List<string> trapNames =
        [
            "Progressive Dynamite",
            "Forbidden Key",
            "Box of Crayons",
            "Progressive Headband",
            "Tippsie Outfit",
            "Ittle Dew 1 Outfit",
            "Jenny Dew Outfit",
            "Swimsuit Outfit",
            "Tiger Jenny Outfit",
            "Little Dude Outfit",
            "Delinquint Outfit",
            "Progressive Melee",
            "Impossible Gates Pass",
            "Raft Piece",
            "Progressive Tracker",
            "Progressive Amulet",
            "Progressive Tome",
            "Progressive Force Wand",
            "Progressive Ice Ring",
            "Progressive Chain"
        ];

        public void ChangePreview(DummyAction dummyAction)
        {
            ItemHandler.ItemData.Item item = ItemRandomizer.Instance.GetItemForLocation(SceneManager.GetActiveScene().name, dummyAction._saveName, out ScoutedItemInfo info);
            string itemName = "";
            if (item == null)
            {
                switch (info.Flags)
                {
                    case ItemFlags.None:
                        itemName = "Filler";
                        break;
                    case ItemFlags.NeverExclude:
                        itemName = "Useful";
                        break;
                    case ItemFlags.Advancement:
                        itemName = "Progression";
                        break;
                }
                Plugin.Log.LogWarning($"Item for preview does not exist, unimplemented? Flags are {info.Flags}");
            }
            else itemName = item.ItemName;
            if (info.Flags == ItemFlags.Trap)
            {
                // specifying int because Unity has a bad habit of using the float version otherwise
                itemName = trapNames[Random.Range((int)0, (int)trapNames.Count)];
            }

            previewObject = FreestandingReplacer.GetModelPreview(itemName);
            if (previewObject == null) return;
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            GameObject newPreview = GameObject.Instantiate(previewObject);
            newPreview.transform.parent = transform;
            newPreview.transform.localPosition = Vector3.zero;
            newPreview.SetActive(true);
        }

        private void OnDisable()
        {
            // Get around Ludo's pooling system by force destroying the key every time it unloads,
            // forcing it to be recreated
            // Chris, if you want to optimize this so this is unneeded, go ahead Mjau
            Destroy(gameObject);
        }
    }
}
