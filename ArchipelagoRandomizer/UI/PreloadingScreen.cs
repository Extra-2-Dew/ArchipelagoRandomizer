using UnityEngine;
using UnityEngine.UI;

namespace ID2.ArchipelagoRandomizer.UI;

public class PreloadingScreen
{
	private readonly GameObject loadingScreenPrefab;
	private GameObject loadingScreen;
	private Slider progressBarSlider;

	public PreloadingScreen()
	{
		// Load asset
		loadingScreenPrefab = BundleLoader.LoadAssetFromBundle<GameObject>("LoadingScreen");
		loadingScreenPrefab.GetComponentInChildren<Slider>().value = 0;
	}

	public void TogglePreloadingScreen(bool show)
	{
		if (show)
		{
			loadingScreen = Object.Instantiate(loadingScreenPrefab);
			progressBarSlider = loadingScreen.GetComponentInChildren<Slider>();
			Object.DontDestroyOnLoad(loadingScreen);
		}
		else if (loadingScreen != null)
		{
			Object.Destroy(loadingScreen);
		}
	}

	public void UpdateProgressBar(float percent)
	{
		if (loadingScreen == null)
		{
			Logger.LogWarning("Tried to update preloading progress bar while loading screen is not active.");
			return;
		}

		progressBarSlider.value = percent / 100;
	}
}