using HarmonyLib;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	[HarmonyPatch]
	public class Patches
	{
		// Prevents loading into saved scene if doing preloading
		[HarmonyPrefix, HarmonyPatch(typeof(MainMenu), nameof(MainMenu.StartGame))]
		public static bool MainMenu_StartGame_Patch(MainMenu __instance)
		{
			if (!ItemRandomizer.IsActive || !Preloader.NeedsPreloading)
				return true;

			__instance.menuImpl.Hide();
			MainMenu.SetupStartGame(__instance._saver, __instance._texts);
			MusicSelector.Instance.StopLayer(__instance._songLayer, __instance._songFadeoutTime);
			return false;
		}

		// Prevents player from spawning during preloading
		[HarmonyPrefix, HarmonyPatch(typeof(PlayerSpawner), nameof(PlayerSpawner.DoSpawn))]
		public static bool PlayerSpawner_DoSpawn_Patch()
		{
			if (ItemRandomizer.IsActive && Preloader.IsPreloading)
				return false;

			return true;
		}

		// Prevents dream dungeons from overriding items if the setting is on
		[HarmonyPrefix]
		[HarmonyPatch(typeof(EntityLocalVarOverrider), nameof(EntityLocalVarOverrider.Apply))]
		public static bool EntityLocalVarOverrider_Apply_Patch()
		{
			return !ItemRandomizer.IsActive || !RandomizerSettings.Instance.KeepItemsInDreamDungeons;
		}

		// Allows for roll to open chests
		[HarmonyPrefix]
		[HarmonyPatch(typeof(RollAction), nameof(RollAction.DoUpdate))]
		public static bool RollAction_DoUpdate_Patch(RollAction __instance)
		{
			if (!ItemRandomizer.IsActive || !__instance.collisionDetector.IsColliding)
				return true;

			ItemRandomizer.Instance.RollToOpenChest(__instance.collisionDetector.GetCollisions());
			return true;
		}

		// Prevents keys from applying to the current scene
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Item), nameof(Item.Pickup))]
		public static bool Item_Pickup_Patch(Item __instance, Entity ent, bool fast)
		{
			// ---------- START CUSTOM CODE ---------- \\

			// If item randomizer is inactive or it's an Entity drop, run original code
			if (!ItemRandomizer.IsActive || __instance.ItemId == null)
				return true;

			for (int i = 0; i < __instance.allComps.Count; i++)
			{
				if (__instance.allComps[i].GetType() == typeof(PickupEffectItem))
					__instance.allComps[i].Apply(ent, fast);
			}

			// ---------- END CUSTOM CODE ---------- \\

			// Saves Entity state
			if (__instance._important)
				ent.SaveState();

			// Saves pickup as picked up
			Item.OnPickedUpFunc onPickedUpFunc = __instance.onPickedUp;
			__instance.onPickedUp = null;

			if (onPickedUpFunc != null)
				onPickedUpFunc(__instance, ent);

			// Deactivates pickup
			__instance.Deactivate();

			return false;
		}

		// Prevents vanilla item preview from spawning out of chests
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Item), nameof(Item.ActivateGraphics))]
		public static bool Item_ActivateGraphics_Patch()
		{
			return !ItemRandomizer.IsActive;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MainMenu), nameof(MainMenu.DoStartMenu))]
		public static bool MainMenu_DoStartMenu_Patch(MainMenu __instance)
		{
			StartMenu.InitGame(__instance._saver, __instance._input, __instance._texts);
			GuiBindInData inData = new(null, null);
			GuiBindData guiBindData;

			if (__instance._layoutIsPrefab)
				guiBindData = GuiNode.CreateAndConnect(__instance._layout, inData);
			else
				guiBindData = GuiNode.Connect(__instance._layout, inData);

			__instance.menuImpl = new MenuImpl<MainMenu>(__instance);
			__instance.menuImpl.AddScreen(new MainMenu.MainScreen(__instance, "startRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.OptionsScreen(__instance, "optionRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.FileSelectScreen(__instance, "fileSelectRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.FileStartScreen(__instance, "fileStartRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.NewGameScreen(__instance, "enterNameRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.DeleteConfirmScreen(__instance, "deleteRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.ExtrasScreen(__instance, "extrasRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.LangScreen(__instance, "langRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.SoundTestScreen(__instance, "soundTestRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.GalleryScreen(__instance, "galleryRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.SaveWarnScreen(__instance, "savewarnRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.RecordsScreen(__instance, "recordsRoot", guiBindData));
			__instance.menuImpl.AddScreen(new MainMenu.CardsScreen(__instance, "cardsRoot", guiBindData));
			__instance.menuImpl.AddScreen(new APMenuStuff.APSettingsScreen(__instance, "apRoot", guiBindData));
			__instance.menuImpl.ShowFirst();

			__instance._onStart?.FireOnActivate(true);

			return false;
		}

		// Replace key with model of randomized item
		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpawnItemEventObserver), nameof(SpawnItemEventObserver.SpawnItem))]
		public static void SpawnItemEventObserver_SpawnItem_Postfix(SpawnItemEventObserver __instance)
		{
			if (!ItemRandomizer.IsActive) return;
			if (__instance.transform.parent.name == "KeyChest")
			{
				GameObject key = __instance.showItem.gameObject;
				PreviewItemData preview = key.AddComponent<PreviewItemData>();
				preview.ChangePreview(__instance.GetComponentInParent<DummyAction>());
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MenuScreen<MainMenu>), nameof(MenuScreen<MainMenu>.Show))]
		public static void MenuScreen_MainMenu_Show_Patch(MenuScreen<MainMenu> __instance)
		{
			switch (__instance.Name.Substring(0, __instance.Name.Length - 4))
			{
				// New game screen
				case "enterName":
					APMenuStuff.Instance.ShowAPButton();
					break;
				// File selected screen
				case "fileStart":
					APMenuStuff.Instance.ShowAPButton(false, true);
					break;
				// File list screen
				case "fileSelect":
					APHandler.Instance.Disconnect();
					break;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MenuScreen<MainMenu>), nameof(MenuScreen<MainMenu>.Hide))]
		public static void MenuScreen_MainMenu_Hide_Patch(MenuScreen<MainMenu> __instance)
		{
			if (__instance.Name == "enterNameRoot" || __instance.Name == "fileStartRoot")
				APMenuStuff.Instance.HideAPButton();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MainMenu.FileStartScreen), nameof(MainMenu.FileStartScreen.ClickedStart))]
		public static bool MainMenu_FileStartScreen_ClickedStart_Patch()
		{
			if (Plugin.Instance.APFileData == null)
				return true;

			string errorMessage = string.Empty;
			bool connected = APHandler.Instance.IsConnected || APHandler.Instance.TryCreateSessionAndConnect(Plugin.Instance.APFileData, out errorMessage);
			APMenuStuff.Instance.ShowMessage(errorMessage);

			return connected;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MainMenu.FileStartScreen), nameof(MainMenu.FileStartScreen.ClickedDuplicate))]
		public static void MainMenu_FileStartScreen_ClickedDuplicate_Patch()
		{
			APMenuStuff.Instance.DuplicateAPDataFile();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MainMenu.DeleteConfirmScreen), nameof(MainMenu.DeleteConfirmScreen.ClickedConfirm))]
		public static void MainMenu_DeleteConfirmScreen_ClickedConfirm_Patch()
		{
			APMenuStuff.Instance.DeleteAPDataFile();
		}

		// KEPT AS REFERENCE SINCE THIS WAS PAIN

		//[HarmonyPatch(typeof(SpawnItemEventObserver))]
		//[HarmonyPatch("SpawnItem")]
		//public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		//{
		//	FieldInfo itemRandoField = AccessTools.Field(typeof(Plugin), "ItemRandomizer");
		//	FieldInfo showItemField = AccessTools.Field(typeof(SpawnItemEventObserver), nameof(SpawnItemEventObserver.showItem));
		//	MethodInfo handleItemReplacementMethod = AccessTools.Method(typeof(ItemRandomizer), "HandleItemReplacement");

		//	return new CodeMatcher(instructions)
		//		// Looks for matches
		//		.MatchForward(true,
		//			// Looks for method call for Item.Pickup
		//			new CodeMatch(OpCodes.Ldloc_0)
		//		).Insert(
		//			// Store Item rando in stack
		//			new CodeInstruction(OpCodes.Ldsfld, itemRandoField),
		//			// Store SpawnItemEventObserver in stack
		//			new CodeInstruction(OpCodes.Ldarg_0),
		//			// Store entity in stack
		//			new CodeInstruction(OpCodes.Ldloc_0),
		//			// Store item in stack
		//			new CodeInstruction(OpCodes.Ldloc_1),
		//			// Calls ItemRandomizer.HandleItemReplacement() which returns the new Item object
		//			new CodeInstruction(OpCodes.Callvirt, handleItemReplacementMethod),
		//			// Reassign item
		//			new CodeInstruction(OpCodes.Stloc_1)
		//		).InstructionEnumeration();
		//}
	}
}