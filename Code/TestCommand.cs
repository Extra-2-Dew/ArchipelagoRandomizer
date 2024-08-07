using ModCore;
using System.IO;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class TestCommand
    {
        private static TestCommand instance;
        public static TestCommand Instance { get { return instance; } }

        private string[] rimColors =
        {
            "ChestDarkYellow",
            "ChestDarkYellow",
            "ChestGrey",
            "ChestGrey",
            "ChestGrey",
            "ChestDarkYellow",
            "ChestGrey",
            "ChestDarkYellow"
        };
        private string[] rimShines =
        {
            "ChestRimGold",
            "ChestRimGold",
            "ChestRimGrey",
            "ChestRimGrey",
            "ChestRimGrey",
            "ChestRimGold",
            "ChestRimGrey",
            "ChestRimGold"
        };
        private string[] chestColors =
        {
            "ChestTealGreen",
            "ChestBrown",
            "ChestBurgundy",
            "ChestDarkGrey",
            "ChestTealGreen",
            "ChestBlue",
            "ChestOrange",
            "ChestOrange"
        };
        private Material rimMaterial;
        private Material chestMaterial;

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
            if (rimMaterial == null)
            {
                Renderer chestRenderer = GameObject.Find("Dungeon_Chest").GetComponentInChildren<Renderer>();
                rimMaterial = chestRenderer.sharedMaterials[1];
                chestMaterial = chestRenderer.sharedMaterials[2];
            }
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int id))
                {
                    rimMaterial.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{rimColors[id]}.png"));
                    rimMaterial.SetTexture("_SpecularRamp", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{rimShines[id]}.png"));
                    chestMaterial.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{chestColors[id]}.png"));
                }
            }
        }
    }
}
