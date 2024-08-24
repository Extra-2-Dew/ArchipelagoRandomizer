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

		private void ActivateCreditsTrigger()
		{
			GameObject winGameTrigger = GameObject.Find("WinGameTrigger");
			winGameTrigger.transform.Find("ActivateTrigger").gameObject.SetActive(false);
			GameObject creditsDoor = winGameTrigger.transform.Find("LevelDoor").gameObject;
			creditsDoor.SetActive(true);

			if (effectRef == null)
				return;

			EffectEventObserver effect = creditsDoor.AddComponent<EffectEventObserver>();
			effect._onEffect = effectRef._onEffect;
			effect.OnFire(false);
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

			// If completed other goals, activate credits trigger
			if (currentGoal == GoalSettings.QueenOfAdventure && HasCompletedQueenOfAdventure)
				ActivateCreditsTrigger();
			else if (currentGoal == GoalSettings.QueenOfDreams && HasCompletedQueenOfDreams)
				ActivateCreditsTrigger();
			else if (currentGoal == GoalSettings.PotionHunt && HasCompletedPotionHunt)
				ActivateCreditsTrigger();
		}

		private void OnItemGet(ItemHandler.ItemData.Item item, string sentFromPlayerName)
		{
			// If not in Fluffy, we don't need to worry about instantly checking for win condition
			if (SceneManager.GetActiveScene().name != "FluffyFields")
				return;

			bool hasCompletedRaftQuest = (currentGoal == GoalSettings.RaftQuest && item.ItemName == "Raft Piece" && HasCompletedRaftQuest);
			bool hasCompletedPotionHunt = (currentGoal == GoalSettings.PotionHunt && item.ItemName == "Potion" && HasCompletedPotionHunt);

			if (hasCompletedRaftQuest || hasCompletedPotionHunt)
				ActivateCreditsTrigger();
		}
	}
}