using UnityEngine;

namespace ArchipelagoRandomizer
{
	public abstract class ItemRandoComponent : MonoBehaviour
	{
		/// <summary>
		/// Preloads any objects this component needs to reference
		/// </summary>
		public virtual void Preload(Preloader preloader)
		{
			//
		}

		/// <summary>
		/// This component is enabled when a file is started or loaded while the randomizer is active
		/// </summary>
		protected virtual void OnEnable()
		{
			//
		}

		/// <summary>
		/// This component is disabled when the randomizer becomes inactive
		/// </summary>
		protected virtual void OnDisable()
		{
			//
		}

		private void Start()
		{
			ItemRandomizer.OnEnabled += OnEnable;
			ItemRandomizer.OnDisabled += () =>
			{
				enabled = false;
			};
			APHandler.Instance.OnDisconnect += OnDisconnected;
		}

		private void OnDisconnected()
		{
			APHandler.Instance.OnDisconnect -= OnDisconnected;
			enabled = false;
		}

		private void OnDestroy()
		{
			ItemRandomizer.OnEnabled -= OnEnable;
			ItemRandomizer.OnDisabled -= OnDisable;
		}
	}
}