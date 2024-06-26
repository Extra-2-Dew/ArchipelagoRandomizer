using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class RandomizedItemData : MonoBehaviour
	{
		public ItemId ItemId { get; set; }
		public Entity Entity { get; set; }
		public string SaveFlag { get; set; }
	}
}