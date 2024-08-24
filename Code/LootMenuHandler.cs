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
        private GuiClickable backButton;
        private Entity player;
        private IDataSaver worldStorage;
        private SaverOwner mainSaver;
        
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

            player = ModCore.Utility.GetPlayer();
            worldStorage = ModCore.Plugin.MainSaver.WorldStorage;
            mainSaver = ModCore.Plugin.MainSaver;

            itemList = transform.Find("ItemList").gameObject;

            templateIcon = itemList.transform.GetChild(0).gameObject;
            lootButtons = new();

            descriptionHolder = transform.Find("Data/ItemData").gameObject;
            noDescriptionHolder = descriptionHolder.transform.parent.Find("NoItem").gameObject;
            descTitle = descriptionHolder.transform.Find("ItemName").GetComponent<TextMesh>();
            descDesc = descriptionHolder.transform.Find("ItemDesc").GetComponent<BetterTextMesh>();

            CreateLootMenu();
            UpdateQuanties(LootMenuType.Loot);

            backButton = transform.Find("BackBtn").GetComponent<GuiClickable>();
            backButton.OnClicked += SwitchToLootMenu;
            lootButtons["KeyBag"].GetComponentInChildren<GuiClickable>().OnClicked += SwitchToKeysMenu;
            lootButtons["Wardrobe"].GetComponentInChildren<GuiClickable>().OnClicked += SwitchToOutfitMenu;
        }

        private void OnEnable()
        {
            if (itemList != null) UpdateQuanties(LootMenuType.Loot);
        }

        public void SetTitleAndDescription(string title, string description)
        {
            noDescriptionHolder.SetActive(false);
            descriptionHolder.SetActive(true);
            descTitle.text = title;
            descDesc.Text = description;
        }

        public void SetEmptyTitleAndDescription()
        {
            noDescriptionHolder.SetActive(true);
            descriptionHolder.SetActive(false);
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
                var i2 = i;
                GameObject button = outfitList.transform.GetChild(i).gameObject;
                var node = outfitMenu.nodes[i];
                CreateButtonNode(button, node);
                button.transform.Find("Item/Root/Background").gameObject.SetActive(false);
                button.transform.GetComponentInChildren<GuiClickable>().OnClicked += () =>
                {
                    ModCore.Utility.GetPlayer().SetStateVariable("outfit", i2);
                    ModCore.Plugin.MainSaver.SaveLocal();
                    backButton.SendClick();
                };
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
            desc.hasQuantity = node.useQuantity;
            //if (node.useQuantity)
            //{
            //    button.transform.Find("Item/Button").gameObject.SetActive(true);
            //    button.transform.Find("Item/Button/Count").gameObject.SetActive(true);
            //}
            button.name = node.name;
            lootButtons.Add(node.name, button);
            Plugin.Log.LogInfo($"Adding {node.name} to list");
        }

        private void SwitchToLootMenu()
        {
            SwitchToMenu(LootMenuType.Loot);
        }

        private void SwitchToKeysMenu()
        {
            lootButtons["KeyBag"].GetComponentInChildren<GuiSelectionObject>().Deselect();
            if (keyList == null) keyList = CreateKeyMenu();
            UpdateQuanties(LootMenuType.Keys);
            SwitchToMenu(LootMenuType.Keys);
        }

        private void SwitchToOutfitMenu()
        {
            lootButtons["Wardrobe"].GetComponentInChildren<GuiSelectionObject>().Deselect();
            if (outfitList == null) outfitList = CreateOutfitMenu();
            UpdateQuanties(LootMenuType.Outfits);
            SwitchToMenu(LootMenuType.Outfits);
        }

        private void SwitchToMenu(LootMenuType menu)
        {
            itemList.SetActive(menu == LootMenuType.Loot);
            keyList?.SetActive(menu == LootMenuType.Keys);
            outfitList?.SetActive(menu == LootMenuType.Outfits);
        }

        private void UpdateQuanties(LootMenuType menu)
        {
            switch (menu)
            {
                case LootMenuType.Loot:
                    lootButtons["FakeEFCS"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(player.GetStateVariable("fakeEFCS"));
                    lootButtons["Wardrobe"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetOutfitCount());
                    lootButtons["Potions"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(player.GetStateVariable("potions"));
                    break;
                case LootMenuType.Keys:
                    lootButtons["DKEYS_PillowFort"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("PillowFort"));
                    lootButtons["DKEYS_SandCastle"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("SandCastle"));
                    lootButtons["DKEYS_ArtExhibit"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("ArtExhibit"));
                    lootButtons["DKEYS_TrashCave"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("TrashCave"));
                    lootButtons["DKEYS_FloodedBasement"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("FloodedBasement"));
                    lootButtons["DKEYS_PotassiumMine"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("PotassiumMine"));
                    lootButtons["DKEYS_BoilingGrave"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("BoilingGrave"));
                    lootButtons["DKEYS_GrandLibrary"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("GrandLibrary"));
                    lootButtons["DKEYS_SunkenLabyrinth"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("SunkenLabyrinth"));
                    lootButtons["DKEYS_MachineFortress"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("MachineFortress"));
                    lootButtons["DKEYS_DarkHypostyle"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("DarkHypostyle"));
                    lootButtons["DKEYS_TombOfSimulacrum"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("TombOfSimulacrum"));
                    lootButtons["DKEYS_DreamDynamite"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("DreamDynamite"));
                    lootButtons["DKEYS_DreamFireChain"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("DreamFireChain"));
                    lootButtons["DKEYS_DreamIce"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("DreamIce"));
                    lootButtons["DKEYS_DreamAll"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetKeyCount("DreamAll"));
                    break;
                case LootMenuType.Outfits:
                    // if we ever randomize the default outfit, we'll need to track it here
                    lootButtons["outfitDefault"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(1);
                    lootButtons["outfitTippsie"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(1));
                    lootButtons["outfitOriginal"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(2));
                    lootButtons["outfitJenny"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(3));
                    lootButtons["outfitSwim"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(4));
                    lootButtons["outfitArmor"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(5));
                    lootButtons["outfitCard"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(6));
                    lootButtons["outfitDelinquent"].GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(7));
                    lootButtons["outfitApaFrog"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(8));
                    lootButtons["outfitThatGuy"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(9));
                    lootButtons["outfitJennyBerry"]?.GetComponentInChildren<SE_LootMenuDescription>().UpdateQuantity(GetHasOutfit(10));
                    break;
            }
        }

        private int GetOutfitCount()
        {
            int count = 0;
            for (int i = 1; i < 11; i++)
            {
                if (worldStorage.HasData($"outfit{i}"))
                {
                    count++;
                }
            }
            return count;
        }

        private int GetHasOutfit(int outfitID)
        {
            int hasOutfit = 0;
            if (worldStorage.HasData($"outfit{outfitID}")) hasOutfit = 1;
            return hasOutfit;
        }

        private int GetKeyCount(string sceneName)
        {
            return mainSaver.GetSaver($"/local/levels/{sceneName}/player/vars").LoadInt("localKeys");
        }

        private enum LootMenuType
        {
            Loot,
            Keys,
            Outfits
        }
    }
}
