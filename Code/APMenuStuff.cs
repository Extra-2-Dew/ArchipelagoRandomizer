using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ArchipelagoRandomizer.ItemRandomizer;

namespace ArchipelagoRandomizer
{
	internal class APMenuStuff : MonoBehaviour
	{
		private static APMenuStuff instance;
		private static APSettingsScreen apMenu;
		private static MainMenu mainMenu;
		private static bool hasLoadedAssets;
		private static GameObject menuPrefab;

		private GameObject apMenuObj;
		private Animator apButtonAnim;
		private Animator apMenuAnim;
		private Button apButtonBtn;
		private Button apMenuBackBtn;
		private GameObject apButtonActiveImageObj;
		private GameObject apButtonInactiveImageObj;
		private RectTransform apButtonRectTrans;
		private InputField[] apMenuInputFields;
		private Toggle[] apMenuToggles;

		private MenuScreen<MainMenu> mainScreen;
		private MenuScreen<MainMenu> fileStartScreen;

		public static APMenuStuff Instance { get { return instance; } }

		public static void LoadMenuAssets()
		{
			AssetBundle bundle = AssetBundle.LoadFromFile(BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Assets", "apmenus"));
			menuPrefab = bundle.LoadAsset<GameObject>("APCanvas");
			hasLoadedAssets = true;
		}

		private void Awake()
		{
			instance = this;

			if (!hasLoadedAssets)
				LoadMenuAssets();

			GameObject menuRoot = Instantiate(menuPrefab, transform);
			Transform apButtonObj = menuRoot.transform.GetChild(0);
			apMenuObj = menuRoot.transform.GetChild(1).gameObject;
			apButtonBtn = apButtonObj.GetComponentInChildren<Button>();
			apMenuBackBtn = apMenuObj.transform.Find("Back Button").GetComponent<Button>();
			apButtonAnim = apButtonObj.GetComponent<Animator>();
			apMenuAnim = apMenuObj.GetComponent<Animator>();
			apButtonActiveImageObj = apButtonObj.transform.GetChild(0).Find("Images").GetChild(1).gameObject;
			apButtonInactiveImageObj = apButtonObj.GetChild(0).transform.Find("Images").GetChild(0).gameObject;
			apButtonRectTrans = apButtonAnim.transform.GetChild(0).GetComponent<RectTransform>();
			apMenuInputFields = apMenuObj.GetComponentsInChildren<InputField>();
			apMenuToggles = apMenuObj.GetComponentsInChildren<Toggle>();
		}

		private void OnEnable()
		{
			apButtonBtn.onClick.AddListener(() =>
			{
				// Show AP menu
				mainMenu.menuImpl.SwitchToScreen(apMenu);
			});

			apMenuBackBtn.onClick.AddListener(() =>
			{
				// Go to previous menu, and show AP button
				HideAPMenu();
			});

			apButtonActiveImageObj.SetActive(false);
			apButtonInactiveImageObj.SetActive(true);
		}

		private void OnDisable()
		{
			apButtonBtn.onClick.RemoveAllListeners();
			apMenuBackBtn.onClick.RemoveAllListeners();
		}

		public void ShowAPButton()
		{
			// Position button differently based on if on new game or file start screen
			apButtonRectTrans.anchoredPosition = GetPositionForAPButton();
			Animate(apButtonAnim, 1);
		}

		private void ShowAPMenu()
		{
			HideAPButton();
			// Show AP menu
			Animate(apMenuAnim, 1);
		}

		public void HideAPButton()
		{
			// Hide AP button
			Animate(apButtonAnim, 2);
		}

		private void HideAPMenu()
		{
			// Hide AP menu
			Animate(apMenuAnim, 2);

			// Set AP data
			APFileData apFileData = GetAPFileData();
			Plugin.Instance.SetAPFileData(apFileData);

			// Switch button icon
			apButtonActiveImageObj.SetActive(apFileData != null);
			apButtonInactiveImageObj.SetActive(apFileData == null);

			if (mainScreen == null)
				mainScreen = GetMainMenuScreen("startRoot");

			// Set main screen as previous screen
			mainMenu.menuImpl.currScreen = mainScreen;
			// Switch to previous screen
			apMenu.SwitchToBack();

			// Show AP button
			ShowAPButton();
		}

		/// <summary>
		/// Animates the slide in/out effect for the given Animator
		/// </summary>
		/// <param name="anim">The animator to do this on</param>
		/// <param name="value">0 = slides to right, 1 = on screen, 2 = slides to left</param>
		private void Animate(Animator anim, int value)
		{
			anim.SetInteger("State", value);
		}

		private MenuScreen<MainMenu> GetMainMenuScreen(string name)
		{
			return mainMenu.menuImpl.allScreens.Find(menu => menu.Name == name);
		}

		private APFileData GetAPFileData()
		{
			string server = GetInputFieldText("Server");
			string portStr = GetInputFieldText("Port");
			string slotName = GetInputFieldText("Slot Name");
			string password = GetInputFieldText("Password");
			bool deathlink = GetToggleValue("DeathlinkToggle");
			bool autoEquipOutfits = GetToggleValue("OutfitToggle");

			if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(slotName))
				return null;

			if (!int.TryParse(portStr, out int port))
			{
				Plugin.Log.LogError($"'{portStr}' is not a valid port number!");
				return null;
			}

			return new()
			{
				Server = server,
				Port = port,
				SlotName = slotName,
				Password = password,
				Deathlink = deathlink,
				AutoEquipOutfits = autoEquipOutfits
			};
		}

		private string GetInputFieldText(string nameOfInputFieldObj)
		{
			InputField inputField = apMenuInputFields.FirstOrDefault(x => x.name == nameOfInputFieldObj);
			return inputField != null ? inputField.text : string.Empty;
		}

		private bool GetToggleValue(string nameOfToggleObj)
		{
			Toggle toggle = apMenuToggles.FirstOrDefault(x => x.name == nameOfToggleObj);
			return toggle != null && toggle.isOn;
		}

		private Vector2 GetPositionForAPButton()
		{
			if (fileStartScreen == null)
				fileStartScreen = GetMainMenuScreen("fileStartRoot");

			return mainMenu.menuImpl.currScreen != fileStartScreen ? new Vector2(0, -150) : new Vector2(77, -250);
		}

		public class APSettingsScreen : MenuScreen<MainMenu>
		{
			public APSettingsScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
			{
				mainMenu = owner;
				apMenu = this;
			}

			public override bool DoShow(MenuScreen<MainMenu> previous)
			{
				Instance.ShowAPMenu();
				return false;
			}
		}
	}
}