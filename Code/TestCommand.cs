using ModCore;
using System.IO;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class TestCommand
    {
        private static TestCommand instance;
        public static TestCommand Instance { get { return instance; } }

        private string[] crystalFaceRamps =
        {
            "ChestJewelGreen",
            "ChestJewelBrown",
            "ChestJewelBurgundy",
            "ChestJewelDarkGrey",
            "ChestJewelCyan",
            "ChestJewelBlue",
            "ChestJewelOrange",
            "ChestJewelOrange"
        };
        private string[] crystalFaceRims =
        {
            "ChestCrystalRimGreen",
            "ChestCrystalRimBrown",
            "ChestCrystalRimBurgundy",
            "ChestCrystalRimDarkGrey",
            "ChestCrystalRimCyan",
            "ChestCrystalRimBlue",
            "ChestCrystalRimOrange",
            "ChestCrystalRimOrange"
        };
        private string[] crystalEdgeColors =
        {
            "ChestTealGreen",
            "ChestBurgundy",
            "ChestLightGrey",
            "ChestLightGrey",
            "ChestCyan",
            "ChestBlue",
            "ChestOrange",
            "ChestYellow"
        };
        // Crystal edges
        private Material edgeMaterial;
        // Crystal faces
        private Material faceMaterial;

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
            if (edgeMaterial == null)
            {
                Renderer chestRenderer = GameObject.Find("Dungeon_PuzzleChest").transform.Find("crystal").GetComponent<Renderer>();
                edgeMaterial = chestRenderer.sharedMaterials[1];
                faceMaterial = chestRenderer.sharedMaterials[0];
            }
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int id))
                {
                    edgeMaterial.shader = Shader.Find("Unlit/Texture");
                    edgeMaterial.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{crystalEdgeColors[id]}.png"));

                    // rimMaterial.SetTexture("_RimRamp", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{rimColors[id]}.png"));
                    faceMaterial.SetTexture("_SpecularRamp", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{crystalFaceRims[id]}.png"));
                    faceMaterial.SetTexture("_RimRamp", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{crystalFaceRamps[id]}.png"));
                    //rimMaterial.SetTexture("_SpecularRamp", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{rimShines[id]}.png"));
                    //chestMaterial.SetTexture("_MainTex", ModCore.Utility.GetTextureFromFile($"{PluginInfo.PLUGIN_NAME}/Assets/{chestColors[id]}.png"));
                }
            }
        }
    }
}
