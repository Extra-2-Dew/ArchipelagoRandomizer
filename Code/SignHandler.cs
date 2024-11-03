using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	/// <summary>
	/// Handles granting items when signs are read
	/// </summary>
	public class SignHandler : ItemRandoComponent
	{
		private readonly List<SignData> allSignData = new();
		private List<ItemRandomizer.LocationData.Location> allSignLocations;

		public static SignHandler Instance { get; private set; }

		private void Awake()
		{
			allSignLocations = ItemRandomizer.GetLocationData().FindAll(location => location.LocationName.EndsWith("Sign"));
			Instance = this;
		}

		protected override void OnEnable()
		{
			// Create sign data objects
			foreach (var location in allSignLocations)
			{
				bool isSuperSecretSign = location.Flag.StartsWith("SignDeep");

				// Don't include secret signs if they're not randomized
				if (isSuperSecretSign && !RandomizerSettings.Instance.IncludeSuperSecrets)
					continue;

				SignData sign = new(location.Flag);
				allSignData.Add(sign);
			}

			Events.OnSceneLoaded += OnSceneLoaded;
		}

		protected override void OnDisable()
		{
			Events.OnSceneLoaded -= OnSceneLoaded;
		}

		/// <summary>
		/// Runs when a sign has been read. This grants the randomized item and marks it as read
		/// </summary>
		/// <param name="sign">The Sign that was read</param>
		public void ReadSign(Sign sign)
		{
			string flag = sign._configString._string;
			SignData signData = GetDataForSign(flag);

			// If super secrets are not included, don't get item for cipher signs
			if (signData == null)
				return;

			ItemRandomizer.Instance.LocationChecked(flag, SceneManager.GetActiveScene().name);
			signData.HasRead = true;

			// Remove sparkle particles
			Destroy(sign._configString.transform.Find("Sparkles").gameObject);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			foreach (ConfigString signConfig in Resources.FindObjectsOfTypeAll<ConfigString>())
			{
				// Skip "fake" signs (like NPCs)
				if (signConfig.transform.parent.name != "Doodads")
					continue;

				// Get sign data
				string signFlag = signConfig._string;
				SignData signData = GetDataForSign(signFlag);

				// Do nothing if sign is not randomized or if it's already been read
				if (signData == null || signData.HasRead)
					continue;

				// Add particle effect to unread signs
				GameObject preloadedKey = Preloader.GetPreloadedObject<GameObject>("Preview Card");
				GameObject particles = preloadedKey.transform.Find("Particle System").gameObject;

				GameObject clonedParticles = Instantiate(particles, signConfig.transform);
				clonedParticles.name = "Sparkles";
			}
		}

		private SignData GetDataForSign(string flag)
		{
			return allSignData.Find(sign => sign.Flag == flag);
		}

		private class SignData(string signFlag)
		{
			private bool hasRead;

			public string Flag { get; } = signFlag;
			public bool HasRead
			{
				get
				{
					// Returns true if sign has been read in this session (skips a file read, as it's unnecessary)
					if (hasRead)
						return true;

					// Reads from save file if it hasn't been read in this session
					return ItemRandomizer.HasCheckedLocation(Flag, SceneManager.GetActiveScene().name);
				}
				set { hasRead = value; }
			}
		}
	}
}