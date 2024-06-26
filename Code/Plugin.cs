using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using ModCore;

namespace ArchipelagoRandomizer
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("ModCore")]
	public class Plugin : BaseUnityPlugin
	{
		internal static ManualLogSource Log { get; private set; }

		private void Awake()
		{
			Log = Logger;
			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
			AddCommands();
		}

		private void AddCommands()
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes()
				.Where(type => string.Equals(type.Namespace, GetType().Namespace, StringComparison.Ordinal))
				.ToArray();

            foreach (Type type in types)
            {
				MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				object instance = null;

                foreach (MethodInfo method in methods)
                {
					object[] attributes = method.GetCustomAttributes(typeof(APAttribute), true);

					if (attributes.Length > 0) 
					{
						if (instance == null) instance = Activator.CreateInstance(type);

						APAttribute ap = (APAttribute) attributes[0];
						DebugMenuManager.CommandHandler.CommandFunc commandDelegate = args => method.Invoke(instance, args);
						DebugMenuManager.Instance.CommHandler.AddCommand(ap.CommandName, commandDelegate, ap.CommandAliases);
					}
                }
            }
        }
	}
}