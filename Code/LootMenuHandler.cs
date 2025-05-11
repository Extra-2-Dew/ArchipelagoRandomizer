using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
		private RandomizerSettings settings;

		private const string assetPath = $"{ID2.PluginInfo.PLUGIN_NAME}/Assets/LootMenuIcons/";

		private IEnumerator Start()
		{
			yield return new WaitForEndOfFrame();

			string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, ID2.PluginInfo.PLUGIN_NAME, "Data", "lootMenuData.json");
			if (!ModCore.Utility.TryParseJson<LootMenuData>(path, out menuData))
			{
				Plugin.Log.LogError("Unable to load Loot Menu Data! Custom loot menus will not be available.");
				yield break;
			}

			player = ModCore.Utility.GetPlayer();
			worldStorage = ModCore.Plugin.MainSaver.WorldStorage;
			mainSaver = ModCore.Plugin.MainSaver;
			settings = RandomizerSettings.Instance;

			itemList = transform.Find("ItemList").gameObject;

			templateIcon = itemList.transform.GetChild(0).gameObject;
			lootButtons = new();

			descriptionHolder = transform.Find("Data/ItemData").gameObject;
			noDescriptionHolder = descriptionHolder.transform.parent.Find("NoItem").gameObject;
			descTitle = descriptionHolder.transform.Find("ItemName").GetComponent<TextMesh>();
			descDesc = descriptionHolder.transform.Find("ItemDesc").GetComponent<BetterTextMesh>();

			CreateLootMenu();
			UpdateQuantities(LootMenuType.Loot);

			backButton = transform.Find("BackBtn").GetComponent<GuiClickable>();
			backButton.OnClicked += SwitchToLootMenu;
			lootButtons["KeyBag"].GetComponentInChildren<GuiClickable>().OnClicked += SwitchToKeysMenu;
			lootButtons["Wardrobe"].GetComponentInChildren<GuiClickable>().OnClicked += SwitchToOutfitMenu;
		}

		private void OnEnable()
		{
			if (itemList != null) UpdateQuantities(LootMenuType.Loot);
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

			keyList = CreateKeyMenu();
			outfitList = CreateOutfitMenu();
			keyList.SetActive(false);
			outfitList.SetActive(false);

			if (settings.KeySetting == KeySettings.Keysey)
			{
				Destroy(lootButtons["KeyBag"]);
			}
			if (settings.GoalSetting != GoalSettings.PotionHunt)
			{
				Destroy(lootButtons["Potions"]);
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

			if (!settings.IncludeSecretDungeons)
			{
				Destroy(lootButtons["DKEYS_SunkenLabyrinth"]);
				Destroy(lootButtons["DKEYS_MachineFortress"]);
				Destroy(lootButtons["DKEYS_DarkHypostyle"]);
				if (settings.GoalSetting != GoalSettings.QueenOfAdventure) Destroy(lootButtons["DKEYS_TombOfSimulacrum"]);
			}

			if (!settings.IncludeDreamDungeons)
			{
				Destroy(lootButtons["DKEYS_DreamDynamite"]);
				Destroy(lootButtons["DKEYS_DreamFireChain"]);
				Destroy(lootButtons["DKEYS_DreamIce"]);
				Destroy(lootButtons["DKEYS_DreamAll"]);
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
					if (button.GetComponentInChildren<SE_LootMenuDescription>().GetQuantity() > 0)
					{
						ModCore.Utility.GetPlayer().SetStateVariable("outfit", i2);
						ModCore.Plugin.MainSaver.SaveLocal();
						backButton.SendClick();
					}
				};
			}

			int maxOutfit = 10;
			if (!settings.IncludeSuperSecrets) maxOutfit = 7;

			for (int i = outfitList.transform.childCount - 1; i > maxOutfit; i--)
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
			SE_LootMenuDescription lootInfo = button.transform.GetChild(0).gameObject.GetComponent<SE_LootMenuDescription>();
			if (lootInfo == null) lootInfo = button.transform.GetChild(0).gameObject.AddComponent<SE_LootMenuDescription>();
			lootInfo.titleText = node.title;
			lootInfo.descriptionText = node.description;
			lootInfo.handler = this;
			lootInfo.hasQuantity = node.useQuantity;
			button.name = node.name;
			lootButtons.Add(node.name, button);
			button.transform.Find("Item/Button").gameObject.SetActive(true);
		}

		private void SwitchToLootMenu()
		{
			SwitchToMenu(LootMenuType.Loot);
		}

		private void SwitchToKeysMenu()
		{
			GameObject keyBag = lootButtons["KeyBag"];
			if (keyBag.GetComponentInChildren<SE_LootMenuDescription>().GetQuantity() == 0) return;
			keyBag.GetComponentInChildren<GuiSelectionObject>().Deselect();
			SwitchToMenu(LootMenuType.Keys);
			UpdateQuantities(LootMenuType.Keys);
		}

		private void SwitchToOutfitMenu()
		{
			GameObject wardrobe = lootButtons["Wardrobe"];
			if (wardrobe.GetComponentInChildren<SE_LootMenuDescription>().GetQuantity() == 0) return;
			wardrobe.GetComponentInChildren<GuiSelectionObject>().Deselect();
			SwitchToMenu(LootMenuType.Outfits);
			UpdateQuantities(LootMenuType.Outfits);
		}

		private void SwitchToMenu(LootMenuType menu)
		{
			itemList.SetActive(menu == LootMenuType.Loot);
			keyList?.SetActive(menu == LootMenuType.Keys);
			outfitList?.SetActive(menu == LootMenuType.Outfits);
		}

		private void UpdateQuantities(LootMenuType menu)
		{
			switch (menu)
			{
				case LootMenuType.Loot:
					GetLootInfo("FakeEFCS")?.SetQuantity(player.GetStateVariable("fakeEFCS"));
					GetLootInfo("KeyBag")?.SetQuantity(1);
					GetLootInfo("Wardrobe")?.SetQuantity(GetOutfitCount());
					GetLootInfo("Potions")?.SetQuantity(player.GetStateVariable("potions"));
					break;
				case LootMenuType.Keys:
					GetLootInfo("DKEYS_PillowFort")?.SetQuantity(GetKeyCount("PillowFort"));
					GetLootInfo("DKEYS_SandCastle")?.SetQuantity(GetKeyCount("SandCastle"));
					GetLootInfo("DKEYS_ArtExhibit")?.SetQuantity(GetKeyCount("ArtExhibit"));
					GetLootInfo("DKEYS_TrashCave")?.SetQuantity(GetKeyCount("TrashCave"));
					GetLootInfo("DKEYS_FloodedBasement")?.SetQuantity(GetKeyCount("FloodedBasement"));
					GetLootInfo("DKEYS_PotassiumMine")?.SetQuantity(GetKeyCount("PotassiumMine"));
					GetLootInfo("DKEYS_BoilingGrave")?.SetQuantity(GetKeyCount("BoilingGrave"));
					GetLootInfo("DKEYS_GrandLibrary")?.SetQuantity(GetKeyCount("GrandLibrary"));
					GetLootInfo("DKEYS_SunkenLabyrinth")?.SetQuantity(GetKeyCount("SunkenLabyrinth"));
					GetLootInfo("DKEYS_MachineFortress")?.SetQuantity(GetKeyCount("MachineFortress"));
					GetLootInfo("DKEYS_DarkHypostyle")?.SetQuantity(GetKeyCount("DarkHypostyle"));
					GetLootInfo("DKEYS_TombOfSimulacrum")?.SetQuantity(GetKeyCount("TombOfSimulacrum"));
					GetLootInfo("DKEYS_DreamDynamite")?.SetQuantity(GetKeyCount("DreamDynamite"));
					GetLootInfo("DKEYS_DreamFireChain")?.SetQuantity(GetKeyCount("DreamFireChain"));
					GetLootInfo("DKEYS_DreamIce")?.SetQuantity(GetKeyCount("DreamIce"));
					GetLootInfo("DKEYS_DreamAll")?.SetQuantity(GetKeyCount("DreamAll"));
					break;
				case LootMenuType.Outfits:
					// if we ever randomize the default outfit, we'll need to track it here
					GetLootInfo("outfitDefault")?.SetQuantity(1);
					GetLootInfo("outfitTippsie")?.SetQuantity(GetHasOutfit(1));
					GetLootInfo("outfitOriginal")?.SetQuantity(GetHasOutfit(2));
					GetLootInfo("outfitJenny")?.SetQuantity(GetHasOutfit(3));
					GetLootInfo("outfitSwim")?.SetQuantity(GetHasOutfit(4));
					GetLootInfo("outfitArmor")?.SetQuantity(GetHasOutfit(5));
					GetLootInfo("outfitCard")?.SetQuantity(GetHasOutfit(6));
					GetLootInfo("outfitDelinquent")?.SetQuantity(GetHasOutfit(7));
					GetLootInfo("outfitApaFrog")?.SetQuantity(GetHasOutfit(8));
					GetLootInfo("outfitThatGuy")?.SetQuantity(GetHasOutfit(9));
					GetLootInfo("outfitJennyBerry")?.SetQuantity(GetHasOutfit(10));
					break;
			}
		}

		private SE_LootMenuDescription GetLootInfo(string key)
		{
			if (lootButtons.ContainsKey(key))
			{
				if (lootButtons[key] == null)
				{
					// We destroyed it
					return null;
				}
				var lootDesc = lootButtons[key].GetComponentInChildren<SE_LootMenuDescription>(true);
				if (lootDesc != null)
				{
					return lootDesc;
				}
				else Plugin.Log.LogError($"Somehow {key} does not have a Loot Description component attached!");
				return null;
			}
			Plugin.Log.LogError($"Invalid key \"{key}\" requested");
			return null;
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
