using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class LootMenuHandler : MonoBehaviour
    {
        private TextMesh descTitle;
        private BetterTextMesh descDesc;
        private GameObject descriptionHolder;
        private GameObject noDescriptionHolder;
        private const string assetPath = $"{PluginInfo.PLUGIN_NAME}/Assets/";

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            GameObject templateIcon = transform.GetChild(0).gameObject;

            GameObject igpIcon = Instantiate(templateIcon);
            igpIcon.transform.SetParent(transform, false);
            igpIcon.transform.localPosition = Vector3.zero;
            igpIcon.transform.localRotation = Quaternion.identity;
            igpIcon.transform.localScale = Vector3.one;
            GameObject iconObject = igpIcon.transform.Find("Item/Root/Pic").gameObject;
            iconObject.SetActive(true);
            iconObject.GetComponent<Renderer>().enabled = true;
            iconObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{assetPath}IconFakeEFCS.png"));
            SE_LootMenuDescription desc = igpIcon.transform.GetChild(0).gameObject.AddComponent<SE_LootMenuDescription>();
            desc.titleText = "Impossible Gates Pass";
            desc.descriptionText = "The randomizer devs won't let us have any fun, so they won't let us have the EFCS under normal circumstances. This opens gates that require it instead.";
            desc.handler = this;
        }

        public void SetTitleAndDescription(string title, string description)
        {
            if (descriptionHolder == null)
            {
                Plugin.Log.LogMessage("1");
                descriptionHolder = transform.parent.Find("Data/ItemData").gameObject;
                Plugin.Log.LogMessage("2");
                noDescriptionHolder = descriptionHolder.transform.parent.Find("NoItem").gameObject;
                Plugin.Log.LogMessage("3");
                descTitle = descriptionHolder.transform.Find("ItemName").GetComponent<TextMesh>();
                Plugin.Log.LogMessage("4");
                descDesc = descriptionHolder.transform.Find("ItemDesc").GetComponent<BetterTextMesh>();
                Plugin.Log.LogMessage("5");
            }
            if (descriptionHolder != null)
            {
                noDescriptionHolder.SetActive(false);
                descriptionHolder.SetActive(true);
                descTitle.text = title;
                descDesc.Text = description;
            }
        }
    }
}
