using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class ModifiedEntrance : MonoBehaviour
	{
		private DoorHandler.DoorData.Door doorData;
		private SaverOwner mainSaver;
		private bool hasActivated;

		public void SetDoorData(DoorHandler.DoorData.Door doorData)
		{
			this.doorData = doorData;
		}

		private void Awake()
		{
			mainSaver = ModCore.Plugin.MainSaver;

			ItemRandomizer.OnItemReceived += OnItemReceived;

			enabled = false;
		}

		private void OnEnable()
		{
			if (ShouldBlockConnection())
			{
				gameObject.SetActive(false);
				return;
			}

			hasActivated = true;
		}

		private void OnDestroy()
		{
			ItemRandomizer.OnItemReceived -= OnItemReceived;
		}

		private bool ShouldBlockConnection()
		{
			if (hasActivated)
				return false;

			IDataSaver connectionSaver = mainSaver.GetSaver($"/local/levels/{doorData.SceneName}/player/regionConnections");
			return connectionSaver.LoadInt(doorData.EnableFlag) == 0;
		}

		private bool ShouldBlockConnection(string flag)
		{
			if (hasActivated)
				return false;

			return doorData.EnableFlag == flag;
		}

		private void OnItemReceived(ItemHandler.ItemData.Item item, string sentFromPlayerName)
		{
			if (item.Type != ItemHandler.ItemTypes.RegionConnector || ShouldBlockConnection(item.SaveFlag))
				return;

			gameObject.SetActive(true);
			hasActivated = true;
		}
	}
}