using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using ModCore;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    
    public class APCommand : Attribute
    {
        [AP(commandName: "archipelago", commandAliases: ["ap"])]
        private void SendAPCommand(string[] args)
        {
            Debug.Log("Ok this ran");
            if (args.Length == 0)
            {
                DebugMenuManager.Instance.UpdateOutput("<color=#d94343>USAGE:\n" +
                    "  /connect {server:port} {slot} {password}: Connect to an Archipelago server" +
                    "  !hint {item}: Hint the location of an item" +
                    "  !release: Release all items remaining in your world to other worlds (if you have permission)" +
                    "  !collect: Receive all items that belong to your world (if you have permission)" +
                    "You can also simply type to chat with other players.</color>");
                return;
            }
            if (args[0] == "/connect")
            {
                if (Archipelago.TryCreateSession(args[1], args[2], args[3], out string message))
                {
                    DebugMenuManager.Instance.UpdateOutput($"<color=#539a39>{message}</color>");
                }
                else
                {
                    DebugMenuManager.Instance.UpdateOutput($"<color=#d94343>{message}</color>");
                }

                return;
            }
            

            string combinedArgs = string.Join(" ", args).TrimEnd();

            if (combinedArgs == "") return;

            Archipelago.Session.Socket.SendPacket(new SayPacket() { Text = combinedArgs });
        }
        
    }
}
