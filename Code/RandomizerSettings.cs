using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ArchipelagoRandomizer
{
	public enum DungeonRewardSettings
	{
		Anything,
		Priority,
		Rewards
	}

	public enum GoalSettings
	{
		RaftQuest,
		QueenOfAdventure,
		QueenOfDreams
	}

	public enum KeySettings
	{
		Vanilla,
		Keyrings,
		Keysey
	}

	public enum ShardSettings
	{
		Open,
		Half,
		Vanilla,
		Lockdown
	}

	class RandomizerSettings
	{
		private DungeonRewardSettings dungeonRewardSetting;
		private GoalSettings goalSetting;
		private bool includeDreamDungeons;
		private bool includePortalWorlds;
		private bool includeSecretDungeons;
		private bool includeSuperSecrets;
		private bool keepItemsInDreamDungeons;
		private KeySettings keySetting;
		private bool openD8;
		private bool openDW;
		private bool openS4;
		private List<string> requiredDungeons;
		private bool rollOpensChests;
		private ShardSettings shardSetting;
		private bool startWithTracker;
		private bool startWithWarps;
		private string syncopePianoPuzzle;

		public static RandomizerSettings Instance { get; private set; }

		public DungeonRewardSettings DungeonRewardSetting { get { return dungeonRewardSetting; } }
		public GoalSettings GoalSetting { get { return goalSetting; } }
		public bool IncludeDreamDungeons { get { return includeDreamDungeons; } }
		public bool IncludePortalWorlds { get { return includePortalWorlds; } }
		public bool IncludeSecretDungeons { get { return includeSecretDungeons; } }
		public bool IncludeSuperSecrets { get { return includeSuperSecrets; } }
		public bool KeepItemsInDreamDungeons { get { return keepItemsInDreamDungeons; } }
		public KeySettings KeySetting { get { return keySetting; } }
		public bool OpenD8 { get { return openD8; } }
		public bool OpenDW { get { return openDW; } }
		public bool OpenS4 { get { return openS4; } }
		public List<string> RequiredDungeons { get { return requiredDungeons; } }
		public bool RollOpensChests { get { return rollOpensChests; } }
		public ShardSettings ShardSetting { get { return shardSetting; } }
		public bool StartWithTracker { get { return startWithTracker; } }
		public bool StartWithWarps { get { return startWithWarps; } }
		public string SyncopePianoPuzzle { get { return syncopePianoPuzzle; } }

		public RandomizerSettings()
		{
			Instance = this;
			Dictionary<string, object> settings = ReadSlotData();
			LogSettings(settings);
		}

		private Dictionary<string, object> ReadSlotData()
		{
			return new()
			{
				{ nameof(DungeonRewardSetting), dungeonRewardSetting = (DungeonRewardSettings)APHandler.GetSlotData<long>("dungeon_rewards_setting") },
				{ nameof(GoalSetting), goalSetting = (GoalSettings)APHandler.GetSlotData<long>("goal") },
				{ nameof(IncludeDreamDungeons), includeDreamDungeons = APHandler.GetSlotData<long>("include_dream_dungeons") == 1 },
				{ nameof(IncludePortalWorlds), includePortalWorlds = APHandler.GetSlotData<long>("include_portal_worlds") == 1 },
				{ nameof(IncludeSecretDungeons), includeSecretDungeons = APHandler.GetSlotData<long>("include_secret_dungeons") == 1 },
				{ nameof(IncludeSuperSecrets), includeSuperSecrets = APHandler.GetSlotData<long>("include_super_secrets") == 1 },
				{ nameof(KeepItemsInDreamDungeons), keepItemsInDreamDungeons = APHandler.GetSlotData<long>("dream_dungeons_do_not_change_items") == 1 },
				{ nameof(KeySetting), keySetting = (KeySettings)APHandler.GetSlotData<long>("key_settings") },
				{ nameof(OpenD8), openD8 = APHandler.GetSlotData<long>("open_d8") == 1 },
				{ nameof(OpenDW), openDW = APHandler.GetSlotData<long>("include_dream_dungeons") == 1 && APHandler.GetSlotData<long>("open_dreamworld") == 1 },
				{ nameof(OpenS4), openS4 = APHandler.GetSlotData<long>("open_s4") == 1 },
				{ nameof(RequiredDungeons), requiredDungeons = dungeonRewardSetting != 0 ? APHandler.GetSlotData<JArray>("required_dungeons").ToObject<List<string>>() : new() },
				{ nameof(RollOpensChests), rollOpensChests = APHandler.GetSlotData<long>("roll_opens_chests") == 1 },
				{ nameof(ShardSetting), shardSetting = (ShardSettings)APHandler.GetSlotData<long>("shard_settings") },
				{ nameof(StartWithTracker), startWithTracker = APHandler.GetSlotData<long>("start_with_tracker") == 1 },
				{ nameof(StartWithWarps), startWithWarps = APHandler.GetSlotData<long>("start_with_all_warps") == 1 },
				{ nameof(SyncopePianoPuzzle), syncopePianoPuzzle = APHandler.GetSlotData<string>("piano_puzzle") }
			};
		}

		private void LogSettings(Dictionary<string, object> settings)
		{
			string message = "Randomizer settings:\n";

			foreach (KeyValuePair<string, object> kvp in settings)
			{
				System.Type valueType = kvp.Value.GetType();

				if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
				{
					message += $"	{kvp.Key}:	";
					List<object> value = ((IEnumerable)kvp.Value).Cast<object>().ToList();

					for (int i = 0; i < value.Count; i++)
					{
						message += value[i];

						if (i < value.Count - 1)
							message += ", ";
					}

					message += "\n";
					continue;
				}

				message += $"	{kvp.Key}:	{kvp.Value}\n";
			}

			Plugin.Log.LogInfo(message);
		}
	}
}