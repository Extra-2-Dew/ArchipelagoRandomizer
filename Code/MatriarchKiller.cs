using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class MatriarchKiller : MonoBehaviour, IBC_TriggerEnterListener, IBC_CollisionEventListener
	{
		BC_Collider worseCollider;

		private void Start()
		{
			Plugin.Log.LogInfo("test");
			worseCollider = PhysicsUtility.RegisterColliderEvents(gameObject, this);
		}

		void IBC_TriggerEnterListener.OnTriggerEnter(BC_TriggerData other)
		{
			if (other.collider.name == "PlayerEnt")
			{
				other.collider.GetComponentInChildren<Killable>().SignalDeath();
				Destroy(transform.parent.gameObject);
			}
		}

		private void OnDestroy()
		{
			worseCollider?.UnregisterEventListener(this);
		}
	}
}