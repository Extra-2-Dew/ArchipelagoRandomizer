using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	[HarmonyPatch]
	internal class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(EntityLocalVarOverrider), nameof(EntityLocalVarOverrider.Apply))]
		// Prevents dream dungeons from overriding items if the setting is on
		public static bool EntityLocalVarOverrider_Apply_Patch()
		{
			return !Convert.ToBoolean(APHandler.GetSlotData<long>("dream_dungeons_do_not_change_items"));
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

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RollAction), nameof(RollAction.DoUpdate))]
		public static bool RollAction_DoUpdate_PrePatch(RollAction __instance)
		{
			if (!ItemRandomizer.IsActive || !__instance.collisionDetector.IsColliding)
				return true;

			ItemRandomizer.Instance.RollToOpenChest(__instance.collisionDetector.GetCollisions());
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(UpdateVarsEventObserver), nameof(UpdateVarsEventObserver.UpdateVars))]
		public static bool UpdateVarsEventObserver_UpdateVars_Patch(UpdateVarsEventObserver __instance)
		{
			// Prevent outfit stands from setting their flags
			if (ItemRandomizer.IsActive && __instance.gameObject.name == "Outfit" && SceneManager.GetActiveScene().name != "CandyCoastCaves")
			{
				// Resets world flag for outfit
				int outfitNum = __instance.GetComponent<ExprVarHolder>()._startValues[0].value;
				ModCore.Plugin.MainSaver.WorldStorage.SaveInt($"outfit{outfitNum}", 0);
				ModCore.Plugin.MainSaver.SaveLocal();

				// Store flag reference
				ItemDataForRandomizer itemDataForRando = __instance.gameObject.GetComponent<ItemDataForRandomizer>();
				if (itemDataForRando == null)
					itemDataForRando = __instance.gameObject.AddComponent<ItemDataForRandomizer>();
				itemDataForRando.SceneName = SceneManager.GetActiveScene().name;
				itemDataForRando.SaveFlag = __instance.transform.parent.Find("Activate").GetComponent<DummyAction>()._saveName;

				// Mark location as checked
				ItemRandomizer.Instance.LocationChecked(itemDataForRando.SaveFlag, itemDataForRando.SceneName);
				return false;
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AttackAction), nameof(AttackAction.CanDoAction))]
		public static bool AttackAction_CanDoAction_Patch(AttackAction __instance)
		{
			// Disable stick if you don't have it
			if (ItemRandomizer.IsActive && __instance.ActionName == "firesword")
			{
				Entity playerEnt = EntityTag.GetEntityByName("PlayerEnt");
				return playerEnt.GetStateVariable("melee") >= 0;
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EntityAction), nameof(EntityAction.CanDoAction))]
		public static bool EntityAction_CanDoAction_Patch(EntityAction __instance)
		{
			// Disable roll if you don't have it
			if (ItemRandomizer.IsActive && __instance.ActionName == "roll")
			{
				Entity playerEnt = EntityTag.GetEntityByName("PlayerEnt");
				return playerEnt.GetStateVariable("canRoll") == 1;
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Item), nameof(Item.Pickup))]
		public static bool Item_Pickup_Patch(Item __instance, Entity ent, bool fast)
		{
			// ---------- START CUSTOM CODE ---------- \\

			// If item randomizer is inactive or it's an Entity drop, run original code
			if (!ItemRandomizer.IsActive || __instance.ItemId == null)
				return true;

			ItemDataForRandomizer itemDataForRando = __instance.GetComponent<ItemDataForRandomizer>();
			ItemRandomizer.Instance.LocationChecked(itemDataForRando.SaveFlag, SceneManager.GetActiveScene().name);

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

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SpawnItemEventObserver), nameof(SpawnItemEventObserver.SpawnItem))]
		public static bool SpawnItemEventObserver_SpawnItem_Patch(SpawnItemEventObserver __instance)
		{
			// ---------- START CUSTOM CODE ---------- \\

			// If item randomizer is inactive, run original code
			if (!ItemRandomizer.IsActive)
				return true;

			// ---------- END CUSTOM CODE ---------- \\

			Entity entity = __instance.Triggerer ?? TransformUtility.GetByName<Entity>(__instance._targetEntName);
			Item item = ItemBase.SelectItem(entity, (!(__instance._itemSelector != null)) ? __instance._itemPrefab : __instance._itemSelector);

			if (item == null)
			{
				// ---------- START CUSTOM CODE ---------- \\

				// Store reference to save flag
				ItemRandomizer.Instance.LocationChecked(__instance.GetComponentInParent<DummyAction>()._saveName, SceneManager.GetActiveScene().name);

				// ---------- END CUSTOM CODE ---------- \\

				Debug.LogWarning("Attempt to spawn null item - remove spawner instead");
				return false;
			}

			__instance.startP = __instance.transform.TransformPoint(__instance._spawnOffset);
			__instance.endP = __instance.transform.TransformPoint(__instance._spawnTarget);
			__instance.showItem = ItemFactory.Instance.GetItem(item, __instance._spawnRoot, __instance.startP, !__instance._pickupDirectly, __instance._stateData);
			__instance.startScale = __instance.showItem.transform.localScale;
			__instance.showItem.transform.localScale = __instance.startScale * __instance._startScale;

			// ---------- START CUSTOM CODE ---------- \\

			// Store reference to save flag
			ItemDataForRandomizer itemDataForRando = __instance.showItem.gameObject.GetComponent<ItemDataForRandomizer>();
			if (itemDataForRando == null)
				itemDataForRando = __instance.showItem.gameObject.AddComponent<ItemDataForRandomizer>();
			itemDataForRando.SaveFlag = __instance.GetComponentInParent<DummyAction>()._saveName;

			// ---------- END CUSTOM CODE ---------- \\

			if (__instance._spawnSound != null)
				SoundPlayer.Instance.PlayPositionedSound(__instance._spawnSound, __instance.transform.position);

			FieldInfo eventField = typeof(SpawnItemEventObserver).GetField("OnSpawn", BindingFlags.Instance | BindingFlags.NonPublic);

			if (eventField != null)
			{
				object eventDelegate = eventField.GetValue(__instance);

				if (eventDelegate != null && eventDelegate is Delegate delegateInstance)
					delegateInstance.DynamicInvoke(__instance.showItem, __instance);
			}

			__instance.timer = __instance._spawnTime;
			__instance.enabled = true;

			if (__instance._pickupDirectly)
			{
				if (entity != null)
				{
					__instance.showItem.Pickup(entity, true);
				}
				else
					Debug.Log($"No entity found to spawn the item for {__instance.name}");
			}

			return false;
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