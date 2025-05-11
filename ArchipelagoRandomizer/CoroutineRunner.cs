using System.Collections;
using UnityEngine;

namespace ID2.ArchipelagoRandomizer;

class CoroutineRunner : MonoBehaviour
{
	private static CoroutineRunner instance;

	public static CoroutineRunner Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject obj = new("CoroutineRunner");
				instance = obj.AddComponent<CoroutineRunner>();
				DontDestroyOnLoad(obj);
			}

			return instance;
		}
	}

	public static Coroutine Start(IEnumerator routine)
	{
		return Instance.StartCoroutine(routine);
	}
}