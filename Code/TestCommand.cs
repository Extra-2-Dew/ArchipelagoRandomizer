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

        [DebugMenuCommand(commandName:"test", caseSensitive: true)]
        private void SendTestCommand(string[] args)
        {
            string connectionName = "Connection - ";
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    connectionName += arg + " ";
                }
            }
            connectionName.TrimEnd(' ');

            BlockadeVisualsHandler.DisableBlockades(connectionName);
        }
    }
}
