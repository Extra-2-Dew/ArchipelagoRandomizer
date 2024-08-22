using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace ArchipelagoRandomizer
{
    public class LootMenuHandler : MonoBehaviour
    {
        private TextMesh descTitle;
        private BetterTextMesh descDesc;
        private GameObject descriptionHolder;
        private GameObject noDescriptionHolder;
        private GameObject templateIcon;
        private GameObject itemList;
        private GameObject keyList;
        private GameObject outfitList;
        private LootMenuData menuData;
        private Dictionary<string, GameObject> lootButtons;
        private const string assetPath = $"{PluginInfo.PLUGIN_NAME}/Assets/LootMenuIcons/";

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Data", "lootMenuData.json");
            if (!ModCore.Utility.TryParseJson<LootMenuData>(path, out menuData))
            {
                Plugin.Log.LogError("Unable to load Loot Menu Data! Custom loot menus will not be available.");
                yield break;
            }

            itemList = transform.Find("ItemList").gameObject;

            templateIcon = itemList.transform.GetChild(0).gameObject;
            lootButtons = new();

            CreateLootMenu();

            transform.Find("BackBtn").GetComponent<GuiClickable>().OnClicked += SwitchToLootMenu;
            lootButtons["KeyBag"].GetComponentInChildren<GuiClickable>().OnClicked += SwitchToKeysMenu;
            lootButtons["Wardrobe"].GetComponentInChildren<GuiClickable>().OnClicked += SwitchToOutfitMenu;
        }

        public void SetTitleAndDescription(string title, string description)
        {
            if (descriptionHolder == null)
            {
                descriptionHolder = transform.Find("Data/ItemData").gameObject;
                noDescriptionHolder = descriptionHolder.transform.parent.Find("NoItem").gameObject;
                descTitle = descriptionHolder.transform.Find("ItemName").GetComponent<TextMesh>();
                descDesc = descriptionHolder.transform.Find("ItemDesc").GetComponent<BetterTextMesh>();
            }
            if (descriptionHolder != null)
            {
                noDescriptionHolder.SetActive(false);
                descriptionHolder.SetActive(true);
                descTitle.text = title;
                descDesc.Text = description;
            }
        }

        private void CreateLootMenu()
        {
            LootMenuData.LootSubMenu lootMenu = menuData.menus.FirstOrDefault((x) => x.menuName == "Loot");

            foreach (var node in lootMenu.nodes)
            {
                GameObject button = Instantiate(templateIcon);
                button.transform.SetParent(itemList.transform, false);
                button.transform.localRotation = Quaternion.identity;
                button.transform.localScale = Vector3.one;
                //GameObject iconObject = button.transform.Find("Item/Root/Pic").gameObject;
                //iconObject.SetActive(true);
                //iconObject.GetComponent<Renderer>().enabled = true;
                //iconObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{assetPath}{node.iconPath}.png"));
                //SE_LootMenuDescription desc = button.transform.GetChild(0).gameObject.AddComponent<SE_LootMenuDescription>();
                //desc.titleText = node.title;
                //desc.descriptionText = node.description;
                //desc.handler = this;
                //if (node.useQuantity)
                //{
                //    button.transform.Find("Item/Button").gameObject.SetActive(true);
                //    button.transform.Find("Item/Button/Count").gameObject.SetActive(true);
                //}
                //lootButtons.Add(node.name, button);
                CreateButtonNode(button, node);
            }
        }

        private GameObject CreateKeyMenu()
        {
            GameObject keyList = Instantiate(itemList);
            keyList.name = "KeyList";
            keyList.transform.SetParent(transform);
            keyList.transform.localPosition = itemList.transform.localPosition;
            keyList.transform.localRotation = Quaternion.identity;
            keyList.transform.localScale = Vector3.one;

            LootMenuData.LootSubMenu keyMenu = menuData.menus.FirstOrDefault((x) => x.menuName == "Keys");

            for (int i = 0; i < 16; i++)
            {
                GameObject button = keyList.transform.GetChild(i).gameObject;
                var node = keyMenu.nodes[i];
                CreateButtonNode(button, node);
            }

            return keyList;
        }

        private GameObject CreateOutfitMenu()
        {
            GameObject outfitList = Instantiate(itemList);
            outfitList.name = "OutfitList";
            outfitList.transform.SetParent(transform);
            outfitList.transform.localPosition = itemList.transform.localPosition;
            outfitList.transform.localRotation = Quaternion.identity;
            outfitList.transform.localScale = Vector3.one;

            LootMenuData.LootSubMenu outfitMenu = menuData.menus.FirstOrDefault((x) => x.menuName == "Outfits");

            for (int i = 0; i < 11; i++)
            {
                GameObject button = outfitList.transform.GetChild(i).gameObject;
                var node = outfitMenu.nodes[i];
                CreateButtonNode(button, node);
                button.transform.Find("Item/Root/Background").gameObject.SetActive(false);
            }
            for (int i = outfitList.transform.childCount -1; i >= 11; i--)
            {
                Destroy(outfitList.transform.GetChild(i).gameObject);
            }

            return outfitList;
        }

        private void CreateButtonNode(GameObject button, LootMenuData.LootMenuNode node)
        {
            GameObject buttonPic = button.transform.Find("Item/Root/Pic").gameObject;
            buttonPic.SetActive(true);
            buttonPic.GetComponent<Renderer>().enabled = true;
            buttonPic.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{assetPath}{node.iconPath}.png"));
            SE_LootMenuDescription desc = button.transform.GetChild(0).gameObject.GetComponent<SE_LootMenuDescription>();
            if (desc == null) desc = button.transform.GetChild(0).gameObject.AddComponent<SE_LootMenuDescription>();
            desc.titleText = node.title;
            desc.descriptionText = node.description;
            desc.handler = this;
            if (node.useQuantity)
            {
                button.transform.Find("Item/Button").gameObject.SetActive(true);
                button.transform.Find("Item/Button/Count").gameObject.SetActive(true);
            }
            button.name = node.name;
            lootButtons.Add(node.name, button);
        }

        private void SwitchToLootMenu()
        {
            SwitchToMenu(LootMenuType.Loot);
        }

        private void SwitchToKeysMenu()
        {
            lootButtons["KeyBag"].GetComponentInChildren<GuiSelectionObject>().Deselect();
            if (keyList == null) keyList = CreateKeyMenu();
            SwitchToMenu(LootMenuType.Keys);
        }

        private void SwitchToOutfitMenu()
        {
            lootButtons["Wardrobe"].GetComponentInChildren<GuiSelectionObject>().Deselect();
            if (outfitList == null) outfitList = CreateOutfitMenu();
            SwitchToMenu(LootMenuType.Outfits);
        }

        private void SwitchToMenu(LootMenuType menu)
        {
            itemList.SetActive(menu == LootMenuType.Loot);
            keyList?.SetActive(menu == LootMenuType.Keys);
            outfitList?.SetActive(menu == LootMenuType.Outfits);
        }

        private enum LootMenuType
        {
            Loot,
            Keys,
            Outfits
        }
    }
}
