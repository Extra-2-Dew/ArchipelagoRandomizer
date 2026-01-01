using System;
using System.Linq;

namespace ID2.ArchipelagoRandomizer;

/// <summary>
/// A BepInEx logging utility class.
/// </summary>
internal static class Logger
{
	/// <summary>
	/// Logs a message to the BepInEx console.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Log(object message)
	{
		Plugin.Logger.LogMessage(message);
	}

	/// <summary>
	/// Logs a message to the BepInEx console.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogInfo(object message)
	{
		Plugin.Logger.LogInfo(message);
	}

	/// <summary>
	/// Logs a warning to the BepInEx console.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogWarning(object message)
	{
		Plugin.Logger.LogWarning(message);
	}

	/// <summary>
	/// Logs an error to the BepInEx console.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="includeTrace">Should the stack trace be printed after the message?</param>
	public static void LogError(object message, bool includeTrace = true)
	{
		Plugin.Logger.LogError(includeTrace ? message + "\n" + GetStackTrace() : message);
	}

	/// <summary>
	/// Logs an exception to the BepInEx console.
	/// </summary>
	/// <param name="ex">The exception to log.</param>
	public static void LogError(Exception ex)
	{
		LogError(ex.ToString(), includeTrace: false);
	}

	private static string GetStackTrace()
	{
		string stack = Environment.StackTrace;
		string[] lines = stack.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		return string.Join("\n", lines.Skip(3).ToArray());
	}
}