using ID2.ItemChanger;
using UnityEngine;

namespace ID2.ArchipelagoRandomizer;

/// <summary>
/// A location that can be broken to receive an item. (eg. barrels, pots, cracked walls, etc.)
/// </summary>
class BreakableLocation : Location
{
	private static Color outlineColor;
	private const string outlineColorStr = "#ffff00";

	public BreakableLocation(string name, Area area, string flag) : base(name, area, flag)
	{
		ColorUtility.TryParseHtmlString(outlineColorStr, out outlineColor);
	}

	/* PATCHES COMMENTED OUT UNTIL USED
	[HarmonyPatch]
	private class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(HitTrigger), nameof(HitTrigger.OnFire))]
		private static void HitTriggerOnFirePatch(HitTrigger __instance)
		{
			if (__instance.name != "Destructible")
			{
				return;
			}

			string flag = Replacer.Instance.GetFlagFromPosition("Destructible", __instance.transform.position);
			Logger.Log(flag);
			if (Replacer.Instance.TryGetLocationFromFlag(flag, out Location location))
			{
				Replacer.Instance.LocationChecked(location);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(HitTrigger), nameof(HitTrigger.OnEnable))]
		private static void HitTriggerOnEnablePatch(HitTrigger __instance)
		{
			if (__instance.name != "Destructible")
			{
				return;
			}

			Material outlineMat = __instance.transform.parent.GetComponentInChildren<MeshRenderer>().materials.ToList().Find(x => x.name.StartsWith("Black"));

			if (outlineMat == null)
			{
				return;
			}

			outlineMat.color = outlineColor;
		}
	}
	*/
}