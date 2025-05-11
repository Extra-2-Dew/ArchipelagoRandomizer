using UnityEngine;
using AP = ID2.ArchipelagoRandomizer.Archipelago;

namespace ID2.ArchipelagoRandomizer;

class ArchipelagoRandomizerMod
{
	private static readonly ArchipelagoRandomizerMod instance = new();
	private GameObject modObj;

	public static ArchipelagoRandomizerMod Instance => instance;

	private ArchipelagoRandomizerMod() { }

	private void ConnectToArchipelago(AP.APSaveData apSaveData)
	{
		try
		{
			if (AP.Instance.Connect(apSaveData) != null)
			{
				modObj = new GameObject("ArchipelagoRandomizer");
				//modObj.AddComponent<ItemRandomizer>();
				Object.DontDestroyOnLoad(modObj);
			}
		}
		catch (LoginValidationException ex)
		{
			Logger.LogError(ex.Message);
		}
	}

	private void EnableMod()
	{
		AP.APSaveData apSaveData = AP.Instance.GetAPSaveData();
		//ConnectToArchipelago()
	}

	private void DisableMod()
	{
		Object.Destroy(modObj);
		AP.Instance.Disconnect();
	}
}