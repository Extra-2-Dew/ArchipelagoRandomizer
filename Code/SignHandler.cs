using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	public class SignHandler
	{
		private readonly List<SignData> allSignData = new();

		public static SignHandler Instance { get; private set; }

		public SignHandler()
		{
			var allSignLocations = ItemRandomizer.GetLocationData().FindAll(location => location.LocationName.EndsWith("Sign"));

			foreach (var location in allSignLocations)
			{
				bool isSuperSecretSign = location.Flag.StartsWith("SignDeep");

				if (isSuperSecretSign && !RandomizerSettings.Instance.IncludeSuperSecrets)
					continue;

				SignData sign = new(location.Flag);
				allSignData.Add(sign);
			}

			Events.OnSceneLoaded += OnSceneLoaded;

			Instance = this;
		}

		public void ReadSign(Sign sign)
		{
			string flag = GetFlagForSign(sign);
			SignData signData = GetDataForSign(flag);

			// If super secrets are not included, don't get item for cipher signs
			if (signData == null)
				return;

			ItemRandomizer.Instance.LocationChecked(flag, SceneManager.GetActiveScene().name);
			signData.HasRead = true;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			foreach (ConfigString signConfig in Resources.FindObjectsOfTypeAll<ConfigString>())
			{
				if (signConfig.transform.parent.name != "Doodads")
					continue;

				Plugin.Log.LogInfo($"ConfigString on {signConfig.gameObject.name}");

				string signFlag = signConfig._string;
				SignData signData = GetDataForSign(signFlag);

				// Do nothing if sign is not randomized or if it's already been read
				if (signData == null || signData.HasRead)
				{
					Plugin.Log.LogInfo($"{signFlag} has been read!");
					signConfig.gameObject.SetActive(false);
					continue;
				}

				// Add particle effect to unread signs
				//
			}
		}

		private string GetFlagForSign(Sign sign)
		{
			return sign._configString._string;
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
					if (hasRead)
						return true;

					Plugin.Log.LogInfo($"Has read? {Flag} at {SceneManager.GetActiveScene().name}");
					return ItemRandomizer.HasCheckedLocation(Flag, SceneManager.GetActiveScene().name);
				}
				set { hasRead = value; }
			}
		}
	}
}