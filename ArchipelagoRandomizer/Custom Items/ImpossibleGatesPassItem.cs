using ID2.ItemChanger;

namespace ID2.ArchipelagoRandomizer;

class ImpossibleGatesPassItem(string displayName) : ICItem(displayName)
{
	public override string Icon => "EFCS";
	public override string Flag => "fakeEFCS";

	public override void Trigger()
	{
		SaverOwner mainSaver = ModCore.Utility.MainSaver;
		mainSaver.GetSaver("/local/levels/TombOfSimulacrum/N").SaveInt("PuzzleDoor_green-100--22", 1);
		mainSaver.GetSaver("/local/levels/TombOfSimulacrum/S").SaveInt("PuzzleDoor_green-64--25", 1);
		mainSaver.GetSaver("/local/levels/TombOfSimulacrum/AC").SaveInt("PuzzleGate-48--54", 1);
		mainSaver.GetSaver("/local/levels/Deep17/B").SaveInt("PuzzleGate-23--5", 1);
		ModCore.Utility.GetPlayer().SetStateVariable(Flag, 1);

		base.Trigger();
	}
}