using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
    public class PreviewItemInfo : MonoBehaviour
    {
        private GameObject previewObject;

        public void ChangePreview(DummyAction dummyAction)
        {
            ItemHandler.ItemData.Item item = ItemRandomizer.Instance.GetItemForLocation(SceneManager.GetActiveScene().name, dummyAction._saveName, out _);
            if (item == null)
            {
                Plugin.Log.LogWarning("Item for preview does not exist, unimplemented?");
                return;
            }

            previewObject = FreestandingReplacer.GetModelPreview(item.ItemName);
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
