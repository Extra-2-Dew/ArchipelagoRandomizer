using ID2.ItemChanger;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ID2.ArchipelagoRandomizer;

class KeyringItem(string displayName, Area forScene) : ICItem(displayName)
{
	private readonly Area forScene = forScene;
	private readonly Dictionary<Area, int> dungeonKeyCounts = new()
	{
		{ Area.PillowFort, 2 },
		{ Area.SandCastle, 2 },
		{ Area.ArtExhibit, 4 },
		{ Area.TrashCave, 4 },
		{ Area.FloodedBasement, 5 },
		{ Area.PotassiumMine, 5 },
		{ Area.BoilingGrave, 5 },
		{ Area.GrandLibrary, 8 },
		{ Area.SunkenLabyrinth, 3 },
		{ Area.MachineFortress, 5 },
		{ Area.DarkHypostyle, 5 },
		{ Area.TombOfSimulacrum, 10 },
		{ Area.DreamDynamite, 3 },
		{ Area.DreamFireChain, 4 },
		{ Area.DreamIce, 4 },
		{ Area.DreamAll, 4 }
	};

	public override string Icon => "Key";
	public override string Flag => "localKeys";

	public override void Trigger()
	{
		// If no keys are found for the dungeon, error
		if (!dungeonKeyCounts.TryGetValue(forScene, out int keyCount))
		{
			Logger.LogError($"Dungeon {forScene} has no keys.");
			return;
		}

		// If not in the scene the key belongs to, save it to level flags
		if (forScene.ToString() != SceneManager.GetActiveScene().name)
		{
			IDataSaver keySaver = ModCore.Utility.MainSaver.GetSaver($"/local/levels/{forScene}/player/vars");
			keySaver.SaveInt(Flag, keyCount);
		}
		// If in the scene the key belongs to, save it to player vars so it updates instantly
		else
		{
			Entity player = ModCore.Utility.GetPlayer();
			player.SetStateVariable(Flag, keyCount);
		}

		base.Trigger();
	}
}