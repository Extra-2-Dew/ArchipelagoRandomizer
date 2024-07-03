﻿using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	[HarmonyPatch]
	internal class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Item), "Pickup")]
		public static bool Item_Pickup_Patch(Item __instance, Entity ent, bool fast)
		{
			// ---------- START CUSTOM CODE ---------- \\

			// If item randomizer is inactive, run original code
			if (!ItemRandomizer.Instance.IsActive)
				return true;

			ItemDataForRandomizer itemDataForRando = __instance.GetComponent<ItemDataForRandomizer>();
			ItemRandomizer.Instance.LocationChecked(itemDataForRando);

			// ---------- END CUSTOM CODE ---------- \\

			if (__instance._important)
				ent.SaveState();

			__instance.Deactivate();

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SpawnItemEventObserver), "SpawnItem")]
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
			itemDataForRando.Entity = entity;
			itemDataForRando.Item = __instance.showItem;
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