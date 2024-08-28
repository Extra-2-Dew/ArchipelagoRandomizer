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
            string obj = "Forbidden Key";
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "0":
                        obj = "Pillow Fort Key";
                        break;
                    case "1":

                        obj = "Sand Castle Key";
                        break;
                    case "2":
                        obj = "Art Exhibit Key";
                        break;
                    case "3":
                        obj = "Trash Cave Key";
                        break;
                    case "4":
                        obj = "Flooded Basement Key";
                        break;
                    case "5":
                        obj = "Potassium Mine Key";
                        break;
                    case "6":
                        obj = "Boiling Grave Key";
                        break;
                    case "7":
                        obj = "Grand Library Key";
                        break;
                    case "8":
                        obj = "Sunken Labyrinth Key";
                        break;
                    case "9":
                        obj = "Machine Fortress Key";
                        break;
                    case "10":
                        obj = "Dark Hypostyle Key";
                        break;
                    case "11":
                        obj = "Tomb of Simulacrum Key";
                        break;
                    case "12":
                        obj = "Syncope Key";
                        break;
                    case "13":
                        obj = "Antigram Key";
                        break;
                    case "14":
                        obj = "Bottomless Tower Key";
                        break;
                    case "15":
                        obj = "Quietus Key";
                        break;
                }
            }
            GameObject logo = GameObject.Instantiate(FreestandingReplacer.GetModelPreview(obj));
            logo.transform.position = ModCore.Utility.GetPlayer().transform.position;
        }
    }
}
