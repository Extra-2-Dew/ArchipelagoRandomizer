using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	class RoomLoadEvents
	{
		private readonly RandomizerSettings settings;

		private LevelRoom CurrentRoom { get; set; }

		public RoomLoadEvents(RandomizerSettings settings)
		{
			this.settings = settings;
			Events.OnRoomChanged += OnRoomChanged;
			Events.OnEntitySpawn += SpeedUpSweatyPathingEnemies;
		}

		public void DoDisable()
		{
			Events.OnRoomChanged -= OnRoomChanged;
			Events.OnEntitySpawn -= SpeedUpSweatyPathingEnemies;
		}

		/// <summary>
		/// Opens Remedy without needing any crayons or needing to wait at all
		/// </summary>
		private void OpenRemedy()
		{
			SpawnObjectEventObserver spawner = CurrentRoom.transform.Find("Doodads/TimedWarperSpawner").GetComponent<SpawnObjectEventObserver>();
			spawner._object.GetComponentInChildren<TimedTouchTrigger>()._time = 0;
			spawner.OnActivate(true);
		}

		/// <summary>
		/// Randomizes the Syncope piano tile puzzle
		/// </summary>
		private void RandomizeSyncopePianoTilePuzzle()
		{
			Transform doodads = CurrentRoom.transform.Find("Doodads");

			// Hint note room
			if (CurrentRoom.RoomName == "W")
			{
				HintHandler.Instance.ShowSyncopePianoPuzzleHint(doodads, settings.SyncopePianoPuzzle);
				return;
			}

			// Piano tile room

			// Destroy sequence stop objects
			foreach (TimerChangeEventObserver stopSeqence in doodads.GetComponentsInChildren<TimerChangeEventObserver>())
			{
				Object.Destroy(stopSeqence.gameObject);
			}

			SequenceTrigger sequenceTrigger = doodads.Find("Sequence").GetComponent<SequenceTrigger>();
			List<TouchTrigger> buttonTriggers = new(doodads.GetComponentsInChildren<TouchTrigger>());
			List<TouchTrigger> whiteKeys = new();
			List<TouchTrigger> blackKeys = new();
			List<TouchTrigger> randomizedKeys = new();
			sequenceTrigger.UnregEvents();
			sequenceTrigger._sequence = buttonTriggers.ToArray();
			sequenceTrigger.RegEvents();

			// Separates white keys and black keys
			foreach (TouchTrigger trigger in buttonTriggers)
			{
				string key = trigger.name.Substring(trigger.name.IndexOf('(') + 1, (trigger.name.Length == 18 ? 1 : 2));

				// If white key
				if (!key.EndsWith("#"))
				{
					whiteKeys.Add(trigger);
					continue;
				}

				// If black key (#)
				blackKeys.Add(trigger);
			}

			// Gets the keys
			foreach (char c in settings.SyncopePianoPuzzle)
			{
				// Gets white key
				if (char.IsUpper(c))
				{
					randomizedKeys.Add(whiteKeys.Find(x => x.name.EndsWith($"({c})")));
					continue;
				}

				// Gets black (#) key
				randomizedKeys.Add(blackKeys.Find(x => x.name.EndsWith($"({char.ToUpper(c)}#)")));
			}

			// Replaces the sequence
			sequenceTrigger._sequence = randomizedKeys.ToArray();
		}

		/// <summary>
		/// Prevents the extra Syncope key from spawning
		/// </summary>
		private void FixSyncopeKeyDupe()
		{
			Transform doodads = CurrentRoom.transform.Find("Doodads");
			SaverOwner mainSaver = ModCore.Plugin.MainSaver;

			if (CurrentRoom.RoomName == "AF")
			{
				if (mainSaver.LevelStorage.GetLocalSaver("AG").LoadInt("KeyChest-71--51") > 0)
					doodads.transform.Find("KeyParent").gameObject.SetActive(false);

				return;
			}

			if (mainSaver.LevelStorage.GetLocalSaver("AF").LoadInt("KeyChest-56--51") > 0)
				doodads.transform.Find("KeyParent").gameObject.SetActive(false);
		}

		/// <summary>
		/// Speeds up sweaty pathing enemies and makes them invincible
		/// </summary>
		private void SpeedUpSweatyPathingEnemies(Entity entity)
		{
			string scene = SceneManager.GetActiveScene().name;
			bool isSweaty = entity.name.StartsWith("Sweaty") && (scene == "FluffyFields" || scene == "FancyRuins" || scene == "FrozenCourt");

			if (isSweaty)
			{
				entity.GetEntityComponent<Moveable>()._moveSpeed = 15;

				if (entity.GetComponent<Hittable>() != null)
					Object.Destroy(entity.GetComponent<Hittable>());
			}
		}

		/// <summary>
		/// Spawns both Frozen Court ghosts regardless of time of day, and prevents
		/// them from despawning when they go offscreen
		/// </summary>
		private void ModifyFrozenGhosts()
		{
			LevelTimeInhibitor[] inhibitors = GameObject.Find("Doors").transform.GetComponentsInChildren<LevelTimeInhibitor>();

			foreach (LevelTimeInhibitor inhibitor in inhibitors)
			{
				Transform parent = inhibitor.transform.parent;
				parent.GetComponent<SpawnEntityEventObserver>()._entity.controller.transform.Find("WalkState").GetComponent<PatrolState>()._canceller = null;
				Object.Destroy(parent.GetComponent<DisableEventObserver>());
				Object.Destroy(inhibitor.gameObject);
			}
		}

		private void OnRoomChanged(Entity entity, LevelRoom toRoom, LevelRoom fromRoom, EntityEventsOwner.RoomEventData data)
		{
			if (toRoom == null)
				return;

			CurrentRoom = toRoom;
			string scene = SceneManager.GetActiveScene().name;

			if (Plugin.IsDebug)
			{
				string debugMsg = "Event fired:\n";

				if (fromRoom != null)
					debugMsg += $"    Prev: {fromRoom.RoomName}\n";
				if (toRoom != null)
					debugMsg += $"    Curr: {toRoom.RoomName}";

				Plugin.LogDebugMessage(debugMsg);
			}

			bool doOpenRemedy = settings.IncludeSuperSecrets && scene == "Deep13" && CurrentRoom.RoomName == "E";
			bool doRandomizeSyncopePianoPuzzle = settings.IncludeDreamDungeons && settings.SyncopePianoPuzzle != "DEAD" && SceneManager.GetActiveScene().name == "DreamDynamite" &&
				(CurrentRoom.RoomName == "K" || CurrentRoom.RoomName == "W");
			bool doFixSyncopeKeyDupe = settings.IncludeDreamDungeons && (toRoom.RoomName == "AF" || toRoom.RoomName == "AG");

			if (doOpenRemedy)
				OpenRemedy();

			if (doRandomizeSyncopePianoPuzzle)
				RandomizeSyncopePianoTilePuzzle();

			if (doFixSyncopeKeyDupe)
				FixSyncopeKeyDupe();

			if (scene == "FrozenCourt")
				ModifyFrozenGhosts();
		}
	}
}