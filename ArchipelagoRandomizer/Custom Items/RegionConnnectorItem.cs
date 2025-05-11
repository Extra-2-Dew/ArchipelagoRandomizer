using ID2.ItemChanger;
using System.Linq;

namespace ID2.ArchipelagoRandomizer;

class RegionConnnectorItem(string displayName, Area area1, Area area2) : ICItem(displayName)
{
	public override void Trigger()
	{
		string flippedFlag = string.Join("_", Flag.Split('_').Reverse().ToArray());
		SaverOwner mainSaver = ModCore.Utility.MainSaver;
		mainSaver.GetSaver($"/local/levels/{area1}/player/regionConnections").SaveInt(Flag, 1);
		mainSaver.GetSaver($"/local/levels/{area2}/player/regionConnections").SaveInt(flippedFlag, 1);

		base.Trigger();
	}
}