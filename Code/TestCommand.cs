using ModCore;
using System;
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

        [DebugMenuCommand(commandName:"test", caseSensitive:true)]
        private void SendTestCommand(string[] args)
        {
            if (args.Length > 0)
            {
                string item = string.Join(" ", args);
                DebugMenuManager.LogToConsole($"You have {ItemHandler.Instance.GetItemCount(ItemHandler.Instance.GetItemData(item), out _)} of {item}");
            }
        }
    }
}
