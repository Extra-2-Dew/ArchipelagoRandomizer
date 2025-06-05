using UnityEngine;

namespace ID2.ArchipelagoRandomizer;

class EntSpawner
{
	public static void SpawnEntity(EntitySpawner spawner, Vector3 position, bool unloadWithRoom = true, bool destroyAfterRoomChange = true)
	{
		if (unloadWithRoom)
		{
			spawner.owner._room = LevelRoom.GetRoomForPosition(position);
		}

		spawner.transform.position = position;
		EntitySpawner instantiatedSpawner = Object.Instantiate(spawner);

		if (destroyAfterRoomChange && instantiatedSpawner.Room != null)
		{
			instantiatedSpawner.Room.OnDeactivateDone += () =>
			{
				Object.Destroy(instantiatedSpawner.gameObject);
			};
		}
	}

	public static void SpawnEntity(SpawnEntityEventObserver entEventObserver, Vector3 position, bool unloadWithRoom = true)
	{
		if (unloadWithRoom)
		{
			entEventObserver.savedRoom = LevelRoom.GetRoomForPosition(position);

			entEventObserver.Room.OnDeactivated += () =>
			{
				entEventObserver.RoomDeactivate();
			};
		}

		entEventObserver.transform.position = position;
		entEventObserver.DoSpawn(entEventObserver._entity);
	}
}