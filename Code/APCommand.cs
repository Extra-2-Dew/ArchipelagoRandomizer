﻿using System;
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
    
    public class APCommand
    {
        [DebugMenuCommand(commandName: "archipelago", commandAliases: ["ap"], caseSensitive:true)]
        private void SendAPCommand(string[] args)
        {
            if (args.Length == 0)
            {
                DebugMenuManager.LogToConsole("USAGE:\n" +
                    "  ap /connect {server:port} {slot} {password}: Connect to an Archipelago server\n" +
                    "  ap !hint {item}: Hint the location of an item\n" +
                    "  ap !release: Release all items remaining in your world to other worlds (if you have permission)\n" +
                    "  ap !collect: Receive all items that belong to your world (if you have permission)\n" +
                    "You can also simply type to chat with other players.", DebugMenuManager.TextColor.Error);
                return;
            }
            if (args[0] == "/connect")
            {
                if (args.Length < 3)
                {
                    DebugMenuManager.LogToConsole("USAGE:\n" +
                        "  ap /connect {server:port} {slot} {password (if needed)}\n" +
                        "Examples:" +
                        "ap /connect localhost:38281 PlayerIttle\n" +
                        "ap /connect archipelago.gg:12345 PlayerIttle mYPassWord", DebugMenuManager.TextColor.Error);
                    return;
                }
                string server = args[1];
                string slot = args[2];
                string password = "";
                if (args.Length >= 4) password = args[3];
                if (Archipelago.TryCreateSession(server, slot, password, out string message))
                {
                    DebugMenuManager.LogToConsole(message, DebugMenuManager.TextColor.Success);
                }
                else
                {
                    DebugMenuManager.LogToConsole(message, DebugMenuManager.TextColor.Error);
                }

                return;
            }
            
            if (Archipelago.session == null)
            {
                DebugMenuManager.LogToConsole("No session active. Please connect with 'ap /connect' first.", DebugMenuManager.TextColor.Error);
                return;
            }

            string combinedArgs = string.Join(" ", args).TrimEnd();

            if (combinedArgs == "") return;

            Archipelago.session.Socket.SendPacket(new SayPacket() { Text = combinedArgs });
        }
        
    }
}
