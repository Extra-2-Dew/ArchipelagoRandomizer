using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	public class GoalHandler : MonoBehaviour
	{
		public static EffectEventObserver effectRef;
		private GoalSettings currentGoal;
		private SaverOwner mainSaver;
		private Entity player;

		private bool HasCompletedRaftQuest
		{
			get
			{
				return player != null && player.GetStateVariable("raft") > 7;
			}
		}
		private bool HasCompletedQueenOfAdventure
		{
			get
			{
				return HasCompletedRaftQuest && player.GetStateVariable("loot") > 0;
			}
		}
		private bool HasCompletedQueenOfDreams
		{
			get
			{
				IDataSaver localSaver = mainSaver.GetSaver("/local/dream");
				return localSaver.GetAllDataKeys().Length > 4;
			}
		}
		private bool HasCompletedPotionHunt
		{
			get
			{
				return player != null && player.GetStateVariable("potions") >= RandomizerSettings.Instance.RequiredPotions;
			}
		}

		private void Awake()
		{
			mainSaver = ModCore.Plugin.MainSaver;
			currentGoal = RandomizerSettings.Instance.GoalSetting;
		}

		private void OnEnable()
		{
			Events.OnSceneLoaded += OnSceneLoaded;
			Events.OnPlayerSpawn += OnPlayerSpawn;
			ItemRandomizer.OnItemReceived += OnItemGet;
		}

		private void OnDisable()
		{
			Events.OnSceneLoaded -= OnSceneLoaded;
			Events.OnPlayerSpawn -= OnPlayerSpawn;
			ItemRandomizer.OnItemReceived -= OnItemGet;
		}

		private void ActivateCreditsTrigger(bool activate)
		{
			GameObject winGameTrigger = GameObject.Find("WinGameTrigger");

			if (activate)
			{
				winGameTrigger.transform.Find("ActivateTrigger").gameObject.SetActive(false);
				GameObject creditsDoor = winGameTrigger.transform.Find("LevelDoor").gameObject;
				creditsDoor.SetActive(true);

				if (effectRef == null)
					return;

				EffectEventObserver effect = creditsDoor.AddComponent<EffectEventObserver>();
				effect._onEffect = effectRef._onEffect;
				effect.OnFire(false);
			}
			else
			{
				winGameTrigger.SetActive(false);
			}
		}

		private void SendCompletion()
		{
			Plugin.Log.LogInfo("Ending reached, sending completion!");
			StatusUpdatePacket statusUpdatePacker = new();
			statusUpdatePacker.Status = ArchipelagoClientState.ClientGoal;
			APHandler.Instance.Session.Socket.SendPacket(statusUpdatePacker);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			// Send completion upon entering Outro scene
			if (scene.name == "Outro")
				SendCompletion();
		}

		private void OnPlayerSpawn(Entity player, GameObject camera, PlayerController controller)
		{
			this.player = player;

			if (SceneManager.GetActiveScene().name != "FluffyFields")
				return;

			bool hasCompletedGoal = currentGoal switch
			{
				GoalSettings.QueenOfAdventure => HasCompletedQueenOfAdventure,
				GoalSettings.QueenOfDreams => HasCompletedQueenOfDreams,
				GoalSettings.PotionHunt => HasCompletedPotionHunt,
				_ => false
			};

			if (hasCompletedGoal)
			{
				ActivateCreditsTrigger(true);
			}
			// Keep win game trigger disabled until all goal requirements are met
			else if (currentGoal != GoalSettings.RaftQuest && HasCompletedRaftQuest)
			{
				ActivateCreditsTrigger(false);
			}
		}

		private void OnItemGet(ItemHandler.ItemData.Item item, string sentFromPlayerName)
		{
			// If not in Fluffy, we don't need to worry about instantly checking for win condition
			if (SceneManager.GetActiveScene().name != "FluffyFields")
				return;

			bool hasCompletedRaftQuest = (currentGoal == GoalSettings.RaftQuest && item.ItemName == "Raft Piece" && HasCompletedRaftQuest);
			bool hasCompletedPotionHunt = (currentGoal == GoalSettings.PotionHunt && item.ItemName == "Potion" && HasCompletedPotionHunt);

			if (hasCompletedRaftQuest || hasCompletedPotionHunt)
				ActivateCreditsTrigger(true);
		}
	}
}