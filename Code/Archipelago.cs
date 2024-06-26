using Archipelago.MultiClient.Net;
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
        public static ArchipelagoSession Session;

        public static bool TryCreateSession(string url, string slot, string password, out string message)
        {
            if (Session != null)
            {
                Session.MessageLog.OnMessageReceived -= OnReceiveMessage;
            }
            try
            {
                Session = ArchipelagoSessionFactory.CreateSession(url);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }

            LoginResult result;

            try
            {
                result = Session.TryConnectAndLogin("Ittle Dew 2", slot, ItemsHandlingFlags.AllItems, password: password);
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
            Session.MessageLog.OnMessageReceived += OnReceiveMessage;
            message = "Successfully connected!";
            return true;
        }

        private static void OnReceiveMessage(LogMessage message)
        {
            DebugMenuManager.LogToConsole(message.ToString());
        }
    }
}
