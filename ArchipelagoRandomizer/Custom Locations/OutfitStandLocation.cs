using HarmonyLib;
using ID2.ItemChanger;

namespace ID2.ArchipelagoRandomizer;

class OutfitStandLocation(string name, Area area, string flag) : Location(name, area, flag)
{
	[HarmonyPatch]
	private class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(TemporaryPauseEventObserver), nameof(TemporaryPauseEventObserver.FireNext))]
		private static bool TemporaryPauseEventObserverFireNextPatch(TemporaryPauseEventObserver __instance)
		{
			if (__instance.name != "Outfit")
			{
				return true;
			}

			string flag = Replacer.Instance.GetFlagFromPosition("OutfitStand", __instance.transform.position);

			if (Replacer.Instance.TryGetLocationFromFlag(flag, out Location location))
			{
				Replacer.Instance.LocationChecked(location);
				return false;
			}

			return true;
		}
	}
}