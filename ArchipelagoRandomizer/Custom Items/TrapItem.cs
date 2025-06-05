using ID2.ItemChanger;
using System.Linq;
using UnityEngine;

namespace ID2.ArchipelagoRandomizer;

class TrapItem : ICItem
{
	public enum TrapType
	{
		Bees,
		BeeOnslaught,
		Matriarch,
		Eruption,
		Debuff,
	}

	private static bool hasPreloaded;
	private readonly TrapType trapType;
	private GameObject beeChestPrefab;
	private SpawnEntityEventObserver beeSwarmSpawner;
	private EntitySpawner matriarchSpawner;
	private LevelEvent eruptionEvent;
	private EntityStatusable debuffStatusable;

	public TrapItem(string displayName, TrapType trapType) : base(displayName)
	{
		this.trapType = trapType;

		if (!hasPreloaded)
		{
			Preloader preloader = Preloader.Instance;

			preloader.AddObjectToPreloadList("MachineFortress", () =>
			{
				Transform levelRoot = GameObject.Find("LevelRoot").transform;
				GameObject beeChest = levelRoot.Find("O/Doodads/Dungeon_ChestBees").gameObject;
				EntityStatusable debuffStatusable = levelRoot.Find("G/Logic/MechaBossLogic/MechabunBossLogic/SpawnerA").GetComponent<SpawnObjectEventObserver>()._object.GetComponent<EntitySpawner>()._entityPrefab.GetComponent<EntityStatusable>();
				debuffStatusable._immunes = ModifyDebuffs(debuffStatusable._immunes);

				return [
					beeChest,
					debuffStatusable
				];
			});
			preloader.AddObjectToPreloadList("VitaminHills2", () =>
			{
				return [
					GameObject.Find("VitaminHillsEventStarter").GetComponent<RoomLevelEventStarter>()._sharedData._events[2].gameObject
				];
			});
			preloader.AddObjectToPreloadList("Deep26", () =>
			{
				return [
					GameObject.Find("MatriarchSpawner")
				];
			});

			hasPreloaded = true;
		}
	}

	public override void Trigger()
	{
		switch (trapType)
		{
			case TrapType.Bees:
				SpawnBeeSwarms(1);
				break;
			case TrapType.BeeOnslaught:
				SpawnBeeSwarms(10);
				break;
			case TrapType.Matriarch:
				SpawnMatriarch();
				break;
			case TrapType.Eruption:
				StartEruption();
				break;
			case TrapType.Debuff:
				ApplyDebuff();
				break;
		}

		base.Trigger();
	}

	private void SpawnBeeSwarms(int count)
	{
		if (beeChestPrefab == null)
		{
			beeChestPrefab = Preloader.GetPreloadedObject<GameObject>("Dungeon_ChestBees");
		}

		if (beeChestPrefab == null)
		{
			Logger.LogError("Failed to preload Bee Swarm spawner, so can't spawn it!");
			return;
		}

		beeSwarmSpawner = beeChestPrefab.GetComponent<SpawnEntityEventObserver>();

		// Spawn count of bee swarms
		for (int i = 0; i < count; i++)
		{
			EntSpawner.SpawnEntity(beeSwarmSpawner, ModCore.Utility.GetPlayer().transform.position);
		}
	}

	private void SpawnMatriarch()
	{
		if (matriarchSpawner == null)
		{
			matriarchSpawner = Preloader.GetPreloadedObject<GameObject>("MatriarchSpawner").GetComponent<EntitySpawner>();

			// Makes Matriarch spawn instantly
			matriarchSpawner._delay = 0;

			// Destroy warper object
			Object.Destroy(matriarchSpawner._entityPrefab.transform.Find("Warper").gameObject);
		}

		if (matriarchSpawner == null)
		{
			Logger.LogError("Failed to preload Matriarch spawner, so can't spawn it!");
			return;
		}

		// Randomize position relative to player's
		float offsetX = Random.value > 0.5f ? 25f : -25f;
		float offsetZ = Random.value > 0.5f ? 25 : -25f;
		Vector3 playerPos = ModCore.Utility.GetPlayer().transform.position;
		Vector3 randomPos = new(playerPos.x + offsetX, playerPos.y, playerPos.z + offsetZ);

		EntSpawner.SpawnEntity(matriarchSpawner, randomPos, false);
	}

	private void StartEruption()
	{
		if (eruptionEvent == null)
		{
			GameObject eruptionEventObj = Preloader.GetPreloadedObject<GameObject>("VolcanoEvent");

			if (eruptionEventObj != null)
			{
				eruptionEvent = eruptionEventObj.GetComponent<LevelEvent>();
			}
		}

		if (eruptionEvent == null)
		{
			Logger.LogError("Failed to preload eruption event, so can't trigger it!");
			return;
		}

		LevelEventMotivator.MotivateEvent(eruptionEvent);
	}

	private void ApplyDebuff()
	{
		if (debuffStatusable == null)
		{
			debuffStatusable = Preloader.GetPreloadedObject<EntityStatusable>("MechabunA");
		}

		if (debuffStatusable == null)
		{
			Logger.LogError("Failed to preload status debuffs!");
			return;
		}

		EntityStatusable playerStatusable = ModCore.Utility.GetPlayer().GetEntityComponent<EntityStatusable>();
		StatusType randomDebuff = debuffStatusable._immunes[Random.Range(0, debuffStatusable._immunes.Length)];
		playerStatusable.AddStatus(randomDebuff);
	}

	private StatusType[] ModifyDebuffs(StatusType[] debuffArray)
	{
		StatusType[] modifiedDebuffs = debuffArray.Where((status) => !status.name.EndsWith("Curse")).ToArray();

		foreach (StatusType status in modifiedDebuffs)
		{
			status._overrides = [];
			status._cancels = [];
			status._data.GetComponent<TimedStatus>()._time = 30;
		}

		return modifiedDebuffs;
	}
}