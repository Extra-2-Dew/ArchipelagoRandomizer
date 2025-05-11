using ID2.ItemChanger;

namespace ID2.ArchipelagoRandomizer;

class WeaponUpgradeItem : ICItem
{
	public WeaponUpgradeItem(string displayName) : base(displayName)
	{
	}

	public override void Trigger()
	{
		string upgradeFlag = GetFlagFromName(DisplayName);
		Entity player = ModCore.Utility.GetPlayer();
		int currentUpgradeLevel = player.GetStateVariable(upgradeFlag);
		int newUpgradeLevel = currentUpgradeLevel < 1 ? 2 : currentUpgradeLevel + 1;

		// Set upgrade level
		player.SetStateVariable(upgradeFlag, newUpgradeLevel);

		// If upgrade is obtained after item, set item level as well
		string itemFlag = upgradeFlag.Replace("upgrade", "");

		if (player.GetStateVariable(itemFlag) > 0)
		{
			player.SetStateVariable(itemFlag, newUpgradeLevel);
		}

		base.Trigger();
	}
}