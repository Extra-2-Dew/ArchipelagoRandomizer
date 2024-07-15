using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class HintHandler : MonoBehaviour
	{
		private long dungeonRewardSetting;
		private List<string> requiredDungeons;

		private void Awake()
		{
			//dungeonRewardSetting = (long)APHandler.slotData["dungeon_rewards_setting"];
			dungeonRewardSetting = APHandler.GetSlotData<long>("dungeon_rewards_setting");
			//requiredDungeons = dungeonRewardSetting != 0 ?
			//	((JArray)APHandler.slotData["required_dungeons"]).ToObject<List<string>>() :
			//	null;
			requiredDungeons = dungeonRewardSetting != 0 ?
				APHandler.GetSlotData<JArray>("required_dungeons").ToObject<List<string>>() :
				null;

			Events.OnEntitySpawn += OnSpawnEntity;
		}

		private void OnDisable()
		{
			requiredDungeons = null;
			Events.OnEntitySpawn -= OnSpawnEntity;
		}

		private void OnSpawnEntity(Entity entity)
		{
			if (requiredDungeons == null)
				return;

			if (entity.name == "NPCJennyMole")
			{
				Sign dialogue = entity.GetComponentInChildren<Sign>();
				dialogue._altStrings = null;
				dialogue._configString = null;
				dialogue._exprData = null;
				dialogue._text = GetDungeonsHint();
			}
		}

		private string GetDungeonsHint()
		{
			string hint = "Going on an adventure? You should check out\n";

			for (int i = 0; i < requiredDungeons.Count; i++)
			{
				if (requiredDungeons.Count > 1 && i == requiredDungeons.Count - 1)
				{
					hint += "and ";
				}
				hint += requiredDungeons[i];
				if (requiredDungeons.Count >= 3 && i < requiredDungeons.Count - 1)
				{
					hint += ", ";
				}
				if (i > 0 && i % 3 == 2 && i < requiredDungeons.Count - 1) hint += "\n";
			}

			hint += ".\nI bet there's some good loot at the end of them.";

			return hint;
		}
	}
}
