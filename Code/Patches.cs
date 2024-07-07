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
		[HarmonyPatch(typeof(UpdateVarsEventObserver), nameof(UpdateVarsEventObserver.UpdateVars))]
		public static bool UpdateVarsEventObserver_UpdateVars_Patch(UpdateVarsEventObserver __instance)
		{
			// Prevent outfit stands from setting their flags
			if (ItemRandomizer.Instance.IsActive && __instance.gameObject.name == "Outfit" && SceneManager.GetActiveScene().name != "CandyCoastCaves")
			{
				// Resets world flag for outfit
				int outfitNum = __instance.GetComponent<ExprVarHolder>()._startValues[0].value;
				ModCore.Plugin.MainSaver.WorldStorage.SaveInt($"outfit{outfitNum}", 0);
				ModCore.Plugin.MainSaver.SaveLocal();

				// Store flag reference
				ItemDataForRandomizer itemDataForRando = __instance.gameObject.GetComponent<ItemDataForRandomizer>();
				if (itemDataForRando == null)
					itemDataForRando = __instance.gameObject.AddComponent<ItemDataForRandomizer>();
				itemDataForRando.SaveFlag = __instance.transform.parent.Find("Activate").GetComponent<DummyAction>()._saveName;

				// Mark location as checked
				ItemRandomizer.Instance.LocationChecked(itemDataForRando.SaveFlag);
				return false;
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AttackAction), nameof(AttackAction.CanDoAction))]
		public static bool AttackAction_CanDoAction_Patch(AttackAction __instance)
		{
			// Disable stick if you don't have it
			if (ItemRandomizer.Instance.IsActive && __instance.ActionName == "firesword")
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
			if (ItemRandomizer.Instance.IsActive && __instance.ActionName == "roll")
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
			if (!ItemRandomizer.Instance.IsActive || __instance.ItemId == null)
				return true;

			ItemDataForRandomizer itemDataForRando = __instance.GetComponent<ItemDataForRandomizer>();
			ItemRandomizer.Instance.LocationChecked(itemDataForRando.SaveFlag);

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

			// ---------- START CUSTOM CODE ---------- \\

			ModCore.Plugin.MainSaver.SaveLocal();

			// ---------- END CUSTOM CODE ---------- \\

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SpawnItemEventObserver), nameof(SpawnItemEventObserver.SpawnItem))]
		public static bool SpawnItemEventObserver_SpawnItem_Patch(SpawnItemEventObserver __instance)
		{
			// ---------- START CUSTOM CODE ---------- \\

			// If item randomizer is inactive, run original code
			if (!ItemRandomizer.Instance.IsActive)
				return true;

			// ---------- END CUSTOM CODE ---------- \\

			Entity entity = __instance.Triggerer ?? TransformUtility.GetByName<Entity>(__instance._targetEntName);
			Item item = ItemBase.SelectItem(entity, (!(__instance._itemSelector != null)) ? __instance._itemPrefab : __instance._itemSelector);

			if (item == null)
			{
				// ---------- START CUSTOM CODE ---------- \\

				// Store reference to save flag
				ItemRandomizer.Instance.LocationChecked(__instance.GetComponentInParent<DummyAction>()._saveName);

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
					__instance.showItem.ActivateGraphics();
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