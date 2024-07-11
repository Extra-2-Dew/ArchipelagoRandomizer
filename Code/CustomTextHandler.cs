using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModCore;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
    public class CustomTextHandler
    {
        public CustomTextHandler()
        {
            Events.OnEntitySpawn += OnSpawnEntity;
        }

        private void OnSpawnEntity(Entity entity)
        {
            if (!ItemRandomizer.Instance.IsActive || (long)APHandler.slotData["dungeon_rewards_setting"] == 0) return;
            if (entity.gameObject.name == "NPCJennyMole")
            {
                Sign dialogue = entity.gameObject.GetComponentInChildren<Sign>();
                dialogue._altStrings = null;
                dialogue._configString = null;
                dialogue._exprData = null;
                dialogue._text = GetDungeonsHint();
            }
        }

        private string GetDungeonsHint()
        {
            string hint = "Going on an adventure? You should check out\n";
            List<string> dungeons = ((JArray)APHandler.slotData["required_dungeons"]).ToObject<List<string>>();
            for (int i = 0; i < dungeons.Count; i++)
            {
                if (dungeons.Count > 1 && i == dungeons.Count - 1)
                {
                    hint += "and ";
                }
                hint += dungeons[i];
                if (dungeons.Count >= 3 && i < dungeons.Count - 1)
                {
                    hint += ", ";
                }
                if (i > 0 && i % 3 == 2 && i < dungeons.Count - 1) hint += "\n";
            }
            hint += ".\nI bet there's some good loot at the end of them.";
            return hint;
        }
    }
}
