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
            string path = BepInEx.Utility.CombinePaths(BepInEx.Paths.PluginPath, PluginInfo.PLUGIN_NAME, "Assets", "apmodels");
            string assetPath = "Assets/Extra2Dew/Prefabs/AP/ItemAPUseful.prefab";
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "0":
                        assetPath = "Assets/Extra2Dew/Prefabs/AP/ItemAPFiller.prefab";
                        break;
                    case "1":
                        assetPath = "Assets/Extra2Dew/Prefabs/AP/ItemAPUseful.prefab";
                        break;
                    case "2":
                        assetPath = "Assets/Extra2Dew/Prefabs/AP/ItemAPProgression.prefab";
                        break;
                    case "3":
                        assetPath = "Assets/Extra2Dew/Prefabs/AP/ItemPotion.prefab";
                        break;
                }
            }
            GameObject logo = GameObject.Instantiate(ModCore.Utility.LoadAssetFromBundle(path, assetPath));
            logo.transform.position = ModCore.Utility.GetPlayer().transform.position;
        }
    }
}
