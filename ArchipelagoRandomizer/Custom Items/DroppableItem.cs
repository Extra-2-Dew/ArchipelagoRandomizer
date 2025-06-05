using ID2.ItemChanger;
using System.Collections.Generic;
using UnityEngine;

namespace ID2.ArchipelagoRandomizer;

class DroppableItem : ICItem
{
	public enum ItemType
	{
		Heart,
		Fruit,
		Lightning,
	}

	private readonly ItemType itemType;
	private List<GameObject> heartPrefabs;
	private List<GameObject> fruitPrefabs;
	private GameObject lightningPrefab;

	public DroppableItem(string displayName, ItemType itemType) : base(displayName)
	{
		this.itemType = itemType;

		Preloader.Instance.AddObjectToPreloadList("BoilingGrave", () =>
		{
			Transform levelRoot = GameObject.Find("LevelRoot").transform;
			Entity chillyRoger = levelRoot.Find("V/Spawners/ChillyRogerSpawner").GetComponent<EntitySpawner>()._entityPrefab;

			// Get item drop references
			EntityDroppable t4Droppable = chillyRoger.GetComponentInChildren<EntityDroppable>();
			GameObject redHeart = t4Droppable._dropTable._items[1].gameObject;
			GameObject blueHeart = t4Droppable._dropTable._items[0].gameObject;
			GameObject lightning = t4Droppable._dropTable._items[4].gameObject;
			GameObject strawberry = t4Droppable._dropTable._items[15].gameObject;
			GameObject yellowHeart = t4Droppable._superTable._items[0].gameObject;
			GameObject banana = t4Droppable._superTable._items[1].gameObject;
			EntityDroppable t3Droppable = levelRoot.Find("R/Spawners/SkullnipSpawner").GetComponent<EntitySpawner>()._entityPrefab.GetComponentInChildren<EntityDroppable>();
			GameObject grapes = t3Droppable._dropTable._items[15].gameObject;
			GameObject apple = t3Droppable._superTable._items[1].gameObject;
			fruitPrefabs = [strawberry, banana, grapes, apple];

			foreach (GameObject fruit in fruitPrefabs)
			{

				/*
				 * 
				 * 
				 * 
				 * 
				 * PRELOAD ENTITYDROPPABLE OBJECT INSTEAD OF STATUSTYPES, SO CHANGES TO THEM DON'T AFFECT VANILLA STATUS APPLICATIONS
				 * 
				 * 
				 * 
				 * 
				 */
				StatusType type = fruit.GetComponent<RandomStatusItem>()._statuses[0];
				type._overrides = [];
				type._cancels = [];
				type._data.GetComponent<TimedStatus>()._time = 60;
			}

			return [
				redHeart, blueHeart, yellowHeart, lightning, strawberry, banana, grapes, apple
			];
		});
		this.itemType = itemType;
	}

	public override void Trigger()
	{
		Vector3 playerPos = ModCore.Utility.GetPlayer().transform.position;

		switch (itemType)
		{
			case ItemType.Heart:
				SpawnHeart(playerPos);
				break;
			case ItemType.Fruit:
				SpawnFruit(playerPos);
				break;
			case ItemType.Lightning:
				SpawnLightning(playerPos);
				break;
		}

		base.Trigger();
	}

	private void SpawnHeart(Vector3 position)
	{
		if (heartPrefabs == null)
		{
			heartPrefabs = [
				Preloader.GetPreloadedObject<GameObject>("Item_Heart"),
				Preloader.GetPreloadedObject<GameObject>("Item_Heart3"),
				Preloader.GetPreloadedObject<GameObject>("Item_Heart4"),
			];
		}

		if (heartPrefabs == null)
		{
			Logger.Log("Failed to preload heart item drops!");
			return;
		}

		GameObject randomHeart = heartPrefabs[Random.Range(0, heartPrefabs.Count)];
		Object.Instantiate(randomHeart).transform.position = position;
	}

	private void SpawnFruit(Vector3 position)
	{
		if (fruitPrefabs == null)
		{
			fruitPrefabs = [
				Preloader.GetPreloadedObject<GameObject>("Item_FruitBanana"),
				Preloader.GetPreloadedObject<GameObject>("Item_FruitApple"),
				Preloader.GetPreloadedObject<GameObject>("Item_FruitGrapes"),
				Preloader.GetPreloadedObject<GameObject>("Item_FruitStrawberry")
			];
		}

		if (fruitPrefabs == null)
		{
			Logger.Log("Failed to preload fruit item drops!");
			return;
		}

		GameObject randomFruit = fruitPrefabs[Random.Range(0, fruitPrefabs.Count)];
		Object.Instantiate(randomFruit).transform.position = position;
	}

	private void SpawnLightning(Vector3 position)
	{
		if (lightningPrefab == null)
		{
			lightningPrefab = Preloader.GetPreloadedObject<GameObject>("Item_LightningBall");
		}

		if (lightningPrefab == null)
		{
			Logger.LogError("Failed to preload lightning drop item!");
			return;
		}

		Object.Instantiate(lightningPrefab).transform.position = position;
	}
}