﻿using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModCore;

namespace ArchipelagoRandomizer
{
    public class Archipelago
    {
        public static ArchipelagoSession session;

        public static bool TryCreateSession(string url, string slot, string password, out string message)
        {
            if (session != null)
            {
                session.MessageLog.OnMessageReceived -= OnReceiveMessage;
            }
            try
            {
                session = ArchipelagoSessionFactory.CreateSession(url);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }

            LoginResult result;

            try
            {
                result = session.TryConnectAndLogin("Ittle Dew 2", slot, ItemsHandlingFlags.AllItems, password: password);
            }
            catch (Exception ex)
            {
                result = new LoginFailure(ex.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to connect to {url} as {slot}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                message = errorMessage;
                return false;
            }

            var loginSuccess = (LoginSuccessful)result;
            session.MessageLog.OnMessageReceived += OnReceiveMessage;
            message = "Successfully connected!\n" +
                "Now that you are connected, you can use !help to list commands to run via the server.";
            return true;
        }

        private static void OnReceiveMessage(LogMessage message)
        {
            DebugMenuManager.LogToConsole(message.ToString());
        }
    }
}
