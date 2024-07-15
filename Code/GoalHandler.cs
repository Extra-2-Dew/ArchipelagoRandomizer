using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	public class GoalHandler : MonoBehaviour
	{
		private void Awake()
		{
			Events.OnSceneLoaded += OnSceneLoaded;
		}

		private void OnDisable()
		{
			Events.OnSceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name != "Outro")
				return;

			Plugin.Log.LogInfo("Ending reached, sending completion.");
			StatusUpdatePacket statusUpdatePacker = new();
			statusUpdatePacker.Status = ArchipelagoClientState.ClientGoal;
			APHandler.Instance.Session.Socket.SendPacket(statusUpdatePacker);
			// TODO: When other goals are added, check for proper goal settings and prerequisites
		}
	}
}
