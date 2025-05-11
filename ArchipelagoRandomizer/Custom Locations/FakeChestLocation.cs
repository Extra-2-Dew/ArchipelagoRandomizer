using HarmonyLib;
using ID2.ItemChanger;
using System.Collections.Generic;

namespace ID2.ArchipelagoRandomizer;

class FakeChestLocation(string name, Area area, string flag) : Location(name, area, flag)
{
	private static readonly List<string> fakeChestNames =
	[
		"Dungeon_ChestBees",
	];

	[HarmonyPatch]
	private class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(SpawnEntityEventObserver), nameof(SpawnEntityEventObserver.OnFire))]
		private static bool SpawnEntityEventObserverOnFirePatch(SpawnEntityEventObserver __instance)
		{
			if (!fakeChestNames.Contains(__instance.name))
			{
				return true;
			}

			string flag = Replacer.Instance.GetFlagFromPosition(__instance.name, __instance.transform.position);

			if (Replacer.Instance.TryGetLocationFromFlag(flag, out Location location))
			{
				Replacer.Instance.LocationChecked(location);
			}

			return false;
		}
	}
}