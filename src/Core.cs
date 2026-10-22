using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using HBS.Logging;
using Newtonsoft.Json;

[assembly: AssemblyVersion("0.1.4")]

namespace BTLeopardStart
{
    public static class Core
    {
        public const string ModName = "BTLeopardStart";
        private static readonly ILog log = Logger.GetLogger(ModName, LogLevel.Debug);

        public static Settings Settings { get; private set; }
        public static string ModDirectory { get; private set; }
        public static Harmony Harmony { get; private set; }

        public static void Init(string directory, string settingsJson)
        {
            ModDirectory = directory;
            Settings = LoadSettings(directory, settingsJson);

            Harmony = new Harmony(ModName);
            Harmony.PatchAll(typeof(Core).Assembly);
            log.Log($"v{VersionString} loaded (ship: {Settings.StartShip}, flashpoints tracked: {Settings.FlashpointIds.Count})");
        }

        /// <summary>ModTek 4.x does not merge the mod's settings.json into the
        /// entry-point settings argument — read the file from the mod directory
        /// directly (same pattern as BTCantinaMissions). The argument is only a
        /// fallback for setups without the file.</summary>
        private static Settings LoadSettings(string directory, string settingsJson)
        {
            var path = Path.Combine(directory, "settings.json");
            try
            {
                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path)) ?? new Settings();
                }
                log.LogWarning($"settings.json not found at {path}, falling back to ModTek-provided settings");
                return JsonConvert.DeserializeObject<Settings>(settingsJson) ?? new Settings();
            }
            catch (Exception e)
            {
                log.LogError($"Settings parse failed: {e.Message}, using defaults");
                return new Settings();
            }
        }

        internal static string VersionString => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        internal static void Log(string message) => log.Log(message);
        internal static void LogWarning(string message) => log.LogWarning(message);
    }

    public class Settings
    {
        /// <summary>"Leopard", "Union", or "Argo" — the ship the career starts on.</summary>
        public string StartShip = "Leopard";

        /// <summary>Tag prefix for the current-ship state. Tags are mutually exclusive:
        /// current_ship_leopard / current_ship_union / current_ship_argo.
        /// Setting one clears the others (handled by the SetSimShip postfix).
        /// Upgrade defs gate on the ship they require (e.g. "current_ship_argo").</summary>
        public string ShipTagPrefix = "current_ship";

        /// <summary>Flashpoints that spawn on the eligible system nearest to the player
        /// instead of a random eligible system anywhere on the map (the Argo-recovery
        /// chain). Empty = placement untouched.</summary>
        public List<string> FlashpointIds = new List<string>();
    }
}
