using System;
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
        
        private void OnEntityDied(Entity entity, Killable.DetailedDeathData data)
        {
            Debug.Log(entity.name);
        }
    }
}
