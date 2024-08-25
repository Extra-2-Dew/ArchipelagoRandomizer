using ModCore;
using System.IO;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class TestCommand
    {
        private static TestCommand instance;
        public static TestCommand Instance { get { return instance; } }


        public TestCommand()
        {
            instance = this;
        }

        public void AddCommands()
        {
            DebugMenuManager.AddCommands(this);
        }

        [DebugMenuCommand(commandName:"test")]
        private void SendTestCommand(string[] args)
        {
            GameObject chest = GameObject.Find("Dungeon_Chest");
            SpawnItemEventObserver observer = chest.GetComponent<SpawnItemEventObserver>();
            ItemSelector selector = (ItemSelector)observer._itemSelector;
            Item item = (Item)selector._data[0].result;
            GameObject chain = GameObject.Instantiate(item.gameObject);
            chain.transform.position = ModCore.Utility.GetPlayer().transform.position + Vector3.up;
        }
    }
}
