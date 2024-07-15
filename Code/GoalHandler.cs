using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Enums;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
    public class GoalHandler
    {
        public GoalHandler()
        {
            Events.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (ItemRandomizer.Instance == null) return;
            if (ItemRandomizer.Instance.IsActive && scene.name == "Outro")
            {
                Plugin.Log.LogInfo("Ending reached, sending completion.");
                var statusUpdatePacker = new StatusUpdatePacket();
                statusUpdatePacker.Status = ArchipelagoClientState.ClientGoal;
                APHandler.Session.Socket.SendPacket(statusUpdatePacker);
                // TODO: When other goals are added, check for proper goal settings and prerequisites
            }
        }
    }
}
