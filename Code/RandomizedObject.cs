using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	class RandomizedObject : RoomEventObserver
	{
		private ObjectType objectType;
		private DummyAction action;
		private string sceneName;
		private bool runBaseAwake = true;

		public string SceneName { get { return sceneName; } }
		public string SaveFlag { get { return action._saveName; } }

		private enum ObjectType
		{
			Card,
			Chest,
			Key,
			OutfitStand
		}

		public override void Awake()
		{
			objectType = GetObjectType();

			switch (objectType)
			{
				case ObjectType.Chest:
					// Get action for crystal chest
					action = transform.GetChild(0).GetComponent<DummyAction>();

					// Get action for non-crystal chest
					if (action == null)
						action = GetComponent<DummyAction>();
					break;
				case ObjectType.Key:
				case ObjectType.Card:
					action = GetComponent<DummyAction>();
					break;
				case ObjectType.OutfitStand:
					action = GetComponentInChildren<DummyAction>();
					TemporaryPauseEventObserver pauser = transform.Find("Outfit").GetComponent<TemporaryPauseEventObserver>();
					pauser._next = [this];
					runBaseAwake = false;
					//Transform changer = transform.Find("Outfit");
					//Destroy(changer.GetComponent<UpdateVarsEventObserver>());
					//Destroy(changer.GetComponent<ExprVarHolder>());
					break;
			}

			if (action == null)
			{
				Plugin.Log.LogError($"Failed to find DummyAction for {gameObject.name}!");
				return;
			}

			sceneName = SceneManager.GetActiveScene().name;

			if (runBaseAwake)
			{
				_action = action;
				base.Awake();
			}

			TryReplaceChestTextures();
		}

		public override void OnFire(bool fast)
		{
			if (fast)
				return;

			ItemObtained();
		}

		private void ItemObtained()
		{
			ItemRandomizer.Instance.LocationChecked(SaveFlag, SceneName);
		}

		private void TryReplaceChestTextures()
		{
			// If rando is inactive or chest match contents is off, do nothing
			if (!ItemRandomizer.IsActive || !Plugin.Instance.APFileData.ChestAppearanceMatchesContents || objectType != ObjectType.Chest)
				return;

			// If object is not a chest, do nothing
			if (GetComponent<SpawnItemEventObserver>() == null && GetComponent<SpawnEntityEventObserver>() == null)
				return;

			// If object is a chest
			Renderer chestMesh = GetComponentInChildren<SkinnedMeshRenderer>();
			Renderer crystalMesh = GetComponentInChildren<MeshRenderer>();
			ChestReplacer.Instance.ReplaceChestTextures(action, chestMesh, crystalMesh);
		}

		private ObjectType GetObjectType()
		{
			switch (gameObject.name)
			{
				case "KeyChest":
					return ObjectType.Key;
				case "CardChest":
					return ObjectType.Card;
				case "DefaultChanger":
					return ObjectType.OutfitStand;
				default:
					return ObjectType.Chest;
			}
		}
	}
}