﻿using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	class SceneLoadEvents
	{
		private readonly RandomizerSettings settings;

		private string SceneName { get; set; }

		private bool DoGiveTempEFCS
		{
			get
			{
				if (!settings.IncludeSuperSecrets || SceneName != "Deep19s")
					return false;

				Entity player = ModCore.Utility.GetPlayer();
				return player.GetStateVariable("melee") == 2 || player.GetStateVariable("fakeEFCS") > 0;
			}
		}

		private bool DoModifyShardReqs
		{
			get
			{
				if (settings.ShardSetting == ShardSettings.Open || settings.ShardSetting == ShardSettings.Vanilla)
					return false;

				return SceneName == "FluffyFields" || SceneName == "FancyRuins" || SceneName == "StarWoods";
			}
		}

		public SceneLoadEvents(RandomizerSettings settings)
		{
			this.settings = settings;
			Events.OnSceneLoaded += OnSceneLoaded;
			ItemRandomizer.OnItemReceived += OnItemReceieved;
		}

		public void DoDisable()
		{
			Events.OnSceneLoaded -= OnSceneLoaded;
			ItemRandomizer.OnItemReceived -= OnItemReceieved;
		}

		/// <summary>
		/// Helps prevent softlocks or near-softlocks when phasing by
		/// resetting your spawn point to the point from before the <br/>
		/// softlock. This applies to cases of entering Tomb of Simulacrum,
		/// the dream dungeons, Cave of Mystery/Ludo City, <br />
		/// or doing Grand Library skip without a way to escape these scenarios
		/// </summary>
		private void OverrideSpawnPoints()
		{
			SceneDoor door = null;

			// Either gets the door with which to modify the spawn point of, or
			// prevents the door from saving, depending on the scene
			switch (SceneName)
			{
				case "LonelyRoad2":
					door = SceneDoor.GetDoorForName("RestorePt1");
					break;
				case "DreamWorld":
					door = SceneDoor.GetDoorForName("DreamWorldInside");
					break;
				case "GrandLibrary":
					door = SceneDoor.GetDoorForName("GrandLibraryInside");
					break;
				case "Deep7":
					SceneDoor.GetDoorForName("Deep17")._saveStartPos = false;
					return;
				case "Deep17":
					SceneDoor.GetDoorForName("Deep18")._saveStartPos = false;
					return;
				case "Deep18":
					SceneDoor.GetDoorForName("Deep18")._saveStartPos = false;
					SceneDoor.GetDoorForName("Deep20")._saveStartPos = false;
					return;
				case "Deep20":
					SceneDoor.GetDoorForName("Deep20")._saveStartPos = false;
					SceneDoor.GetDoorForName("Deep20_2")._saveStartPos = false;
					return;
			}

			// If no door, return
			if (door == null)
				return;

			// Set spawn position and direction
			Vector3 pos = door.transform.TransformPoint(door._spawnOffset);
			Vector3 dir = -door.transform.forward;

			// Update spawn point
			PlayerRespawner.GetActiveInstance().UpdateSpawnPoint(pos, dir, door, false);
		}

		/// <summary>
		/// Gives temporary EFCS
		/// </summary>
		private void GiveTempEFCS()
		{
			Entity player = ModCore.Utility.GetPlayer();

			// Require Fire Mace and Fake EFCS
			if (player.GetStateVariable("melee") < 2 || player.GetStateVariable("fakeEFCS") < 1)
				return;

			player.AddLocalTempVar("melee");
			player.SetStateVariable("melee", 3);
		}

		/// <summary>
		/// Modifes the amount of Secret Shards required to unlock Sunken Labyrinth,
		/// Machine Fortress, and Dark Hypostyle
		/// </summary>
		private void ModifyShardDungeonReqs()
		{
			ExprVarHolder varHolder = GameObject.Find("SecretDungeonDoor").GetComponent<ExprVarHolder>();
			string key = "shardTarget";

			// If half
			if (settings.ShardSetting == ShardSettings.Half)
				varHolder.SetValue(key, varHolder.GetValue(key) / 2);
			// If lockdown
			else if (settings.ShardSetting == ShardSettings.Lockdown)
			{
				int shardCount = 36;

				if (SceneName == "FluffyFields")
					shardCount = 12;
				else if (SceneName == "FancyRuins")
					shardCount = 24;

				varHolder.SetValue(key, shardCount);
			}
		}

		/// <summary>
		/// Opens Dream World
		/// </summary>
		private void OpenDreamWorld()
		{
			GameObject.Find("ShowIt").transform.GetChild(0).GetComponent<EntityExprInhibitor>()._data._inhibitIf = "true";
		}

		/// <summary>
		/// Creates signs with text in Promised Remedy
		/// </summary>
		private void CreateRemedySigns()
		{
			GameObject signBase = GameObject.Find("LevelRoot").transform.Find("F").GetChild(9).gameObject;
			GameObject speechBase = GameObject.Find("LevelRoot").transform.Find("F/Doodads/SpeechBubble").gameObject;
			GameObject reqsSign = GameObject.Instantiate(signBase);
			reqsSign.name = "Requirements Sign";
			GameObject reqsSpeech = GameObject.Instantiate(speechBase);
			reqsSpeech.name = "Speech Bubble";
			reqsSpeech.GetComponent<Sign>()._configString = null;
			reqsSpeech.GetComponent<Sign>()._text = "Only the one who can pass the\nimpossible gates and wields the\nflaming mace may proceed.";
			reqsSpeech.transform.SetParent(reqsSign.transform);
			reqsSpeech.transform.localPosition = Vector3.up * 0.5f;
			reqsSign.transform.SetParent(GameObject.Find("LevelRoot").transform.Find("Q/Doodads"));
			reqsSign.transform.position = new Vector3(49, 0, - 78);
			reqsSign.AddComponent<BC_ColliderAACylinder8>().Extents = Vector2.one * 0.5f;
			GameObject hintSign = GameObject.Instantiate(reqsSign);
			hintSign.name = "Hint Sign";
			string hintPlayer = "";
			string hintItem = "";
			hintSign.GetComponentInChildren<Sign>()._text = $"Deep in the never-ending madness,\nthe way to {hintPlayer}'s {hintItem}\nawaits.";
			hintSign.transform.SetParent(GameObject.Find("LevelRoot").transform.Find("Q/Doodads"));
			hintSign.transform.position = new Vector3(55, 0, -78);
		}

		private void DisableRando()
		{
			MessageBoxHandler.Instance.HideMessageBoxes();
			Object.Destroy(ItemRandomizer.Instance);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			SceneName = scene.name;

			Plugin.LogDebugMessage("Event fired:\n" + $"    Curr: {scene.name}");

			// Disable rando
			if (scene.name == "MainMenu")
			{
				DisableRando();
				return;
			}

			// Hide message boxes when in credits
			if (scene.name == "Outro")
				MessageBoxHandler.Instance.HideMessageBoxes();

			if (settings.IncludeSuperSecrets && scene.name == "Deep19s")
				CreateRemedySigns();

			OverrideSpawnPoints();

			if (DoModifyShardReqs)
				ModifyShardDungeonReqs();

			if (DoGiveTempEFCS)
				GiveTempEFCS();

			// Open DW
			if (settings.OpenDW && scene.name == "FluffyFields")
				OpenDreamWorld();
		}

		private void OnItemReceieved(ItemHandler.ItemData.Item item, string sentFromPlayerName)
		{
			if (item.ItemName == "Raft Piece" && DoGiveTempEFCS)
				GiveTempEFCS();
		}
	}
}