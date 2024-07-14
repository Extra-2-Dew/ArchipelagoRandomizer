using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System.Collections;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class DeathLinkHandler : MonoBehaviour
	{
		private static DeathLinkHandler instance;
		private readonly string[] deathMessages =
		{
			"didn't dew.",
			"was pummelled to death.",
			"grew tired of adventuring.",
			"ran out of hearts.",
			"forgot to pack health potions.",
			"dropped all their crayons."
		};
		private ControllerBase playerController;
		private Killable playerKillable;
		private float deathSafetyTime = 10f;
		private bool deathSafety; // While this is true, you cannot receive or send Death Links
		private bool lastDeathFromDeathLink;

		public static DeathLinkHandler Instance { get { return instance; } }
		private DeathLinkService DeathLinkService { get; set; }

		private void Awake()
		{
			instance = this;
			HandleSubscriptions(true);
			StartService();
		}

		private void HandleSubscriptions(bool subscribe)
		{
			if (subscribe)
			{
				ItemRandomizer.Instance.OnDeactivated += DisableService;
				Events.OnPlayerSpawn += OnPlayerSpawn;
				Events.OnEntityDied += OnEntityDied;
				return;
			}

			ItemRandomizer.Instance.OnDeactivated -= DisableService;
			Events.OnPlayerSpawn -= OnPlayerSpawn;
			Events.OnEntityDied -= OnEntityDied;
		}

		private void StartService()
		{
			DeathLinkService = APHandler.Session.CreateDeathLinkService();
			DeathLinkService.EnableDeathLink();
			DeathLinkService.OnDeathLinkReceived += OnDeathLinkReceived;
			Plugin.Log.LogInfo("Deathlink enabled. Have fun! :)");
		}

		private void DisableService()
		{
			DeathLinkService?.DisableDeathLink();
			HandleSubscriptions(false);
			Plugin.Log.LogInfo("Deathlink disabled.");
		}

		private void OnPlayerSpawn(Entity player, GameObject camera, PlayerController controller)
		{
			playerKillable = player.GetEntityComponent<Killable>();
			StartCoroutine(AssignPlayerController(player));
		}

		private void OnEntityDied(Entity entity, Killable.DetailedDeathData data)
		{
			if (deathSafety || entity.name != "PlayerEnt" || data.deathTag == "warp" || lastDeathFromDeathLink)
				return;

			Plugin.Log.LogMessage("Ittle died! Oh no! Sending death to others!");
			string playerName = APHandler.Instance.CurrentPlayer.Name;
			string randomDeathMessage = GetDeathMessage(data.deathTag);
			string deathMessage = $"{playerName} {randomDeathMessage}";

			DeathLinkService.SendDeathLink(new DeathLink(playerName, deathMessage));
			StartCoroutine(DeathSafetyCounter());
		}

		private string GetDeathMessage(string deathTag)
		{
			if (!string.IsNullOrEmpty(deathTag))
			{
				switch (deathTag)
				{
					case "hole":
						return "fell into a hole.";
					case "spike":
						return "learned that spikes are sharp.";
					case "lava":
						return "plunged into lava.";
				}
			}

			return deathMessages[Random.Range(0, deathMessages.Length)];
		}

		private void OnDeathLinkReceived(DeathLink deathLink)
		{
			if (deathSafety)
				return;

			StartCoroutine(SendDeath(deathLink));
		}

		private IEnumerator SendDeath(DeathLink deathLink)
		{
			yield return new WaitForEndOfFrame();

			// Wait for player to exist
			while (playerController == null)
				yield return null;

			// Wait for player control
			while (ObjectUpdater.Instance.IsPaused(playerController.GetUpdatableLayer()))
				yield return null;

			// Wait for ObjectUpdater to be set
			while (playerController.updInst == null)
				yield return null;

			playerKillable.CurrentHp = 0;
			lastDeathFromDeathLink = true;
			playerKillable.SignalDeath();
			lastDeathFromDeathLink = false;
			string deathMessage = deathLink.Cause != null ? deathLink.Cause : $"{deathLink.Source} died!";

			// If death sent from another player
			if (deathLink.Source != APHandler.Instance.CurrentPlayer.Name)
			{
				ItemMessageHandler.MessageData messageData = new()
				{
					Message = deathMessage,
					DisplayTime = 5
				};
				ItemMessageHandler.Instance.ShowMessageBox(messageData);
			}

			StartCoroutine(DeathSafetyCounter());
			Plugin.Log.LogInfo(deathMessage);
		}

		// Turns off your Death Link invincibility
		private IEnumerator DeathSafetyCounter()
		{
			deathSafety = true;
			yield return new WaitForSeconds(deathSafetyTime);
			deathSafety = false;
		}

		// Delays assignment of PlayerController until a bit after PlayerEnt spawns
		// This avoids issue with sending death immediately upon scene load
		// There's probably a better way to handle this, but this is what I've got for now
		private IEnumerator AssignPlayerController(Entity entity)
		{
			yield return new WaitForSeconds(0.5f);
			playerController = ControllerFactory.Instance.GetControllerForEnt(entity);
		}
	}
}
