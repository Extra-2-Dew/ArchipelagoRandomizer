using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using ModCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class DeathLinkHandler
    {
        public DeathLinkHandler()
        {
            Events.OnEntityDied += OnEntityDied;
        }

        public static DeathLinkService deathLinkService;

        // while this is true, you cannot receive or send Death Links.
        private bool deathSafety = false;
        
        private void OnEntityDied(Entity entity, Killable.DetailedDeathData data)
        {
            if (ItemRandomizer.Instance.IsActive && !deathSafety && entity.name == "PlayerEnt")
            {
                Plugin.Log.LogMessage("Ittle died! Oh no!");
                string deathMessage = $"{APHandler.GetPlayerName()} {RandomDeathString()}";
                deathLinkService.SendDeathLink(new DeathLink(APHandler.GetPlayerName(), deathMessage));
                deathSafety = true;
                DebugMenuManager.LogToConsole(deathMessage);
                Plugin.Instance.StartCoroutine(DeathSafetyCounter());
            }
        }

        public void OnDeathLinkReceived(DeathLink deathLink)
        {
            if (deathSafety) return;
            Killable player = ModCore.Utility.GetPlayer().gameObject?.GetComponentInChildren<Killable>();
            if (player != null)
            {
                player.SignalDeath();
                player.CurrentHp = 0;
                string deathMessage = "";
                if (deathLink.Cause != null) deathMessage = deathLink.Cause;
                else deathMessage = $"{deathLink.Source} died!";
                DebugMenuManager.LogToConsole(deathMessage);
                deathSafety = true;
                Plugin.Instance.StartCoroutine(DeathSafetyCounter());
                Plugin.Log.LogWarning("Chris make an Item Get message for this once you've done the rewrite Mjau");
            }
        }

        private string RandomDeathString()
        {
            string[] deathMessages = 
            {
                "didn't dew.",
                "was pummelled to death.",
                "got tired of adventuring.",
                "ran out of hearts.",
                "forgot to pack health potions."
            };
            return deathMessages[Random.Range((int)0, deathMessages.Length)];
        }

        // turns off your Death Link invincibility
        private IEnumerator DeathSafetyCounter()
        {
            yield return new WaitForSeconds(10);
            deathSafety = false;
        }
    }
}
