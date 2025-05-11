using HarmonyLib;
using ID2.ItemChanger;

namespace ID2.ArchipelagoRandomizer;

class SignLocation : Location
{
	public SignLocation(string name, Area area, string flag) : base(name, area, flag)
	{
	}

	[HarmonyPatch]
	private class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Sign), nameof(Sign.Show))]
		private static bool SignShowPatch(Sign __instance)
		{
			//Vector3 position = __instance.transform.position;
			//string flag = Replacer.Instance.GetFlagFromPosition("Sign", position);
			string flag = __instance._configString._string;

			if (Replacer.Instance.TryGetLocationFromFlag(flag, out Location location))
			{
				Replacer.Instance.LocationChecked(location);
			}

			return true;
		}
	}
}