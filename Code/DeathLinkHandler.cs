using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
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
            if (!deathSafety && entity.name == "PlayerEnt")
            {
                deathLinkService.SendDeathLink(new DeathLink(APHandler.Session.Players.GetPlayerAlias(APHandler.Session.ConnectionInfo.Slot), RandomDeathString()));
                deathSafety = true;
                Plugin.Instance.StartCoroutine(DeathSafetyCounter());
            }
        }

        public void OnDeathLinkReceived(DeathLink deathLink)
        {

        }

        private string RandomDeathString()
        {
            string[] deathMessages = 
            {
                "didn't dew.",
                "was pummelled to death.",
                "got tired of adventuring.",
                "ran out of hearts."
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
