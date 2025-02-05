using Archipelago.MultiClient.Net.Models;
using System.Linq;
using UnityEngine;
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
				if (!settings.IncludeSuperSecrets)
					return false;

				// Require Fire Mace and Fake EFCS
				Entity player = ModCore.Utility.GetPlayer();
				return player.GetStateVariable("melee") == 2 && player.GetStateVariable("fakeEFCS") > 0;
			}
		}

		private bool DoModifyShardReqs
		{
			get
			{
				return settings.ShardSetting == ShardSettings.Half || settings.ShardSetting == ShardSettings.Lockdown;
			}
		}

		public SceneLoadEvents(RandomizerSettings settings)
		{
			this.settings = settings;
			Events.OnSceneLoaded += OnSceneLoaded;
			Events.OnPlayerRespawn += OnPlayerRespawn;
			ItemRandomizer.OnItemReceived += OnItemReceieved;
		}

		public void DoDisable()
		{
			Events.OnSceneLoaded -= OnSceneLoaded;
			Events.OnPlayerRespawn -= OnPlayerRespawn;
			ItemRandomizer.OnItemReceived -= OnItemReceieved;
		}

		private void AddCustomComponentToItems()
		{
			foreach (SpawnItemEventObserver itemSpawner in Resources.FindObjectsOfTypeAll<SpawnItemEventObserver>())
			{
				// Keys and cards
				if (itemSpawner.name == "Spawner")
				{
					itemSpawner.transform.parent.gameObject.AddComponent<RandomizedObject>();
					continue;
				}

				if (itemSpawner.name.Contains("Chest"))
					itemSpawner.gameObject.AddComponent<RandomizedObject>();
			}

			if (SceneName == "FluffyFieldsCaves")
				GameObject.Find("LevelRoot").transform.Find("U/DefaultChanger").gameObject.AddComponent<RandomizedObject>();

			else if (SceneName == "Deep19s" && RandomizerSettings.Instance.IncludeSuperSecrets)
				GameObject.Find("LevelRoot").transform.Find("B/Doodads/DefaultChanger").gameObject.AddComponent<RandomizedObject>();

			else if (SceneName == "MachineFortress")
				GameObject.Find("LevelRoot").transform.Find("O/Doodads/Dungeon_ChestBees").gameObject.AddComponent<RandomizedObject>();
		}

		/// <summary>
		/// Adds Ice damage to the accepted damge types for the Lonely Road meteor that blocks Moon Garden
		/// </summary>
		private void FixLonelyRoadMeteor()
		{
			DamageType iceDamage = Resources.FindObjectsOfTypeAll<DamageType>().First((x) => x.name == "dmg_Cold");
			GameObject.Find("LevelRoot").transform.Find("A/Doors/CaveA (night flames and meteor)/PuzzleStuff/Meteor/BreakableMeteor/Collision").GetComponent<HitTrigger>()._damageTypes.Add(iceDamage);
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
				case "Deep23":
					SceneDoor.GetDoorForName("Deep23")._saveStartPos = false;
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
			player.AddLocalTempVar("melee");
			player.SetStateVariable("melee", 3);
		}

		/// <summary>
		/// Modifes the amount of Secret Shards required to unlock Sunken Labyrinth,
		/// Machine Fortress, and Dark Hypostyle
		/// </summary>
		private void ModifyShardDungeonReqs()
		{
			GameObject secretDungeonDoor = GameObject.Find("SecretDungeonDoor");

			// Return if no door (is disabled after button is pressed)
			if (secretDungeonDoor == null)
				return;

			ExprVarHolder varHolder = secretDungeonDoor.GetComponent<ExprVarHolder>();
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
		/// Creates signs with text in Promised Remedy
		/// </summary>
		private void CreateRemedySigns()
		{
			GameObject signBase = GameObject.Find("LevelRoot").transform.Find("F").GetChild(9).gameObject;
			GameObject speechBase = GameObject.Find("LevelRoot").transform.Find("F/Doodads/SpeechBubble").gameObject;

			// Set up requirements sign
			GameObject reqsSign = Object.Instantiate(signBase);
			reqsSign.name = "Requirements Sign";
			reqsSign.transform.SetParent(GameObject.Find("LevelRoot").transform.Find("Q/Doodads"));
			reqsSign.transform.position = new Vector3(49, 0, -78);
			reqsSign.AddComponent<BC_ColliderAACylinder8>().Extents = Vector2.one * 0.5f;
			reqsSign.AddComponent<Light>().color = new(1, 0.4f, 0.4f);

			// Set up requirements speech bubble
			GameObject reqsSpeech = Object.Instantiate(speechBase);
			reqsSpeech.name = "Speech Bubble";
			reqsSpeech.transform.SetParent(reqsSign.transform);
			reqsSpeech.transform.localPosition = Vector3.up * 0.5f;
			Sign reqsSpeechSign = reqsSpeech.GetComponent<Sign>();
			reqsSpeechSign._configString = null;
			reqsSpeechSign._text = "Only the one who can pass the\nimpossible gates and wields the\nflaming mace may proceed.";

			// Set up hint sign
			GameObject hintSign = Object.Instantiate(signBase);
			hintSign.name = "Hint Sign";
			hintSign.transform.SetParent(GameObject.Find("LevelRoot").transform.Find("Q/Doodads"));
			hintSign.transform.position = new Vector3(55, 0, -78);
			hintSign.transform.localScale = new Vector3(-1, 1, 1);
			hintSign.AddComponent<BC_ColliderAACylinder8>().Extents = Vector2.one * 0.5f;
			hintSign.AddComponent<Light>().color = new(1, 0.4f, 0.4f);

			// Set up hint speech bubble
			GameObject hintSpeech = Object.Instantiate(reqsSpeech);
			hintSpeech.name = "Speech Bubble";
			hintSpeech.transform.SetParent(hintSign.transform);
			hintSpeech.transform.localPosition = Vector3.up * 0.5f;
			ItemRandomizer.Instance.GetItemForLocation("Deep19s", "outfit9", out ScoutedItemInfo itemInfo);
			string player = itemInfo.Player.Slot == APHandler.Instance.CurrentPlayer.Slot ? "your" : itemInfo.Player.Name + "'s";
			hintSpeech.GetComponent<Sign>()._text = $"Deep in the never-ending madness,\nthe way to {player} {itemInfo.ItemDisplayName}\n awaits.";
		}

		/// <summary>
		/// Removes the vanilla bee chest spawner in Machine Fortress.
		/// This allows for that chest to give a random item.
		/// </summary>
		private void RemoveBeeChestSpawner()
		{
			Transform beeChest = GameObject.Find("LevelRoot").transform.Find("O/Doodads/Dungeon_ChestBees");
			Object.Destroy(beeChest.GetComponent<SpawnEntityEventObserver>());
		}

		/// <summary>
		/// Implements various quality of life features
		/// </summary>
		private void QualityOfLifeStuff()
		{
			// Pepperpain Mountain cow UFO for Maze of Steel
			if (SceneName == "VitaminHills3")
			{
				GameObject.Find("Countdown").GetComponent<TimerTrigger>().timer = 0;
				return;
			}
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

			switch (SceneName)
			{
				case "Intro":
					return;
				case "MainMenu":
					DisableRando();
					return;
				case "Outro":
					MessageBoxHandler.Instance.HideMessageBoxes();
					return;
				case "Deep19s":
					if (DoGiveTempEFCS)
						GiveTempEFCS();

					if (settings.IncludeSuperSecrets)
						CreateRemedySigns();
					break;
				case "MachineFortress":
					RemoveBeeChestSpawner();
					break;
				case "FluffyFields":
				case "FancyRuins":
				case "StarWoods":
					if (DoModifyShardReqs)
						ModifyShardDungeonReqs();
					break;
				case "LonelyRoad":
					FixLonelyRoadMeteor();
					break;
			}

			AddCustomComponentToItems();
			OverrideSpawnPoints();

			if (Plugin.Instance.APFileData.QualityOfLife)
				QualityOfLifeStuff();

			if (settings.BlockRegionConnections) BlockadeVisualsHandler.SpawnBlockades(SceneName);
		}

		private void OnPlayerRespawn()
		{
			if (SceneName == "Deep19s" && DoGiveTempEFCS)
				GiveTempEFCS();
		}

		private void OnItemReceieved(ItemHandler.ItemData.Item item, string sentFromPlayerName)
		{
			if ((item.Type == ItemHandler.ItemTypes.Melee || item.Type == ItemHandler.ItemTypes.EFCS) && SceneName == "Deep19s" && DoGiveTempEFCS)
				GiveTempEFCS();
		}
	}
}