namespace ID2.ArchipelagoRandomizer;

static class Logger
{
	public static void Log(object message)
	{
		Plugin.Logger.LogMessage(message);
	}

	public static void LogWarning(object message)
	{
		Plugin.Logger.LogWarning(message);
	}

	public static void LogError(object message)
	{
		Plugin.Logger.LogError(message);
	}
}