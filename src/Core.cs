using System;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

[assembly: AssemblyVersion("0.1.0")]

namespace BTLeopardStart
{
    public static class Core
    {
        public static Settings Settings { get; private set; }
        public static string ModDirectory { get; private set; }
        public static Harmony Harmony { get; private set; }

        public static void Init(string directory, string settingsJson)
        {
            ModDirectory = directory;
            try
            {
                Settings = JsonConvert.DeserializeObject<Settings>(settingsJson) ?? new Settings();
            }
            catch (Exception e)
            {
                Debug.LogError($"[BTLeopardStart] Settings parse failed: {e.Message}, using defaults");
                Settings = new Settings();
            }

            Harmony = new Harmony("BTLeopardStart");
            Harmony.PatchAll(typeof(Core).Assembly);
            Debug.Log($"[BTLeopardStart] v{VersionString} loaded (ship: {Settings.StartShip})");
        }

        internal static string VersionString => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        internal static void Log(string message) => Debug.Log($"[BTLeopardStart] {message}");
        internal static void LogWarning(string message) => Debug.LogWarning($"[BTLeopardStart] {message}");
    }

    public class Settings
    {
        /// <summary>"Leopard", "Union", or "Argo" — the ship the career starts on.</summary>
        public string StartShip = "Leopard";

        /// <summary>Tag prefix for the current-ship state. Tags are mutually exclusive:
        /// current_ship_leopard / current_ship_union / current_ship_argo.
        /// Setting one clears the others (handled by transition events).
        /// Upgrade defs gate on the ship they require (e.g. "current_ship_argo").</summary>
        public string ShipTagPrefix = "current_ship";
    }
}
