using System;
using System.Collections.Generic;
using BattleTech;
using HarmonyLib;

namespace BTLeopardStart.Patches
{
    /// <summary>
    /// Career start-ship switch. The native flow sets the career ship to Argo
    /// inside _OnAttachUXComplete — after the sim scene is loaded (earlier,
    /// SpaceController does not exist yet and SetSimShip would NRE). Postfixing
    /// here means our switch runs after and wins over the native one. Save loads
    /// and combat returns take the other branch of that method, which re-asserts
    /// the persisted ship; the SetSimShip postfix mirrors that into the tags.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    public static class SimGameState_OnAttachUXComplete
    {
        public static void Postfix(SimGameState __instance)
        {
            if (__instance.IsFromSave || !__instance.IsCareerMode() || __instance.DaysPassed != 0)
            {
                return; // only the fresh career start, never saves or the campaign
            }
            if (__instance.CurDropship == DropshipType.Leopard)
            {
                return;
            }

            if (WantsLeopard(__instance))
            {
                __instance.SetSimShip(DropshipType.Leopard);
                Core.Log("fresh career: starting ship switched to Leopard");
            }
        }

        /// <summary>A "StartingDropship" option picked in the career difficulty
        /// screen (parked in ConstantOverrides and saved with the career) wins
        /// over the mod settings default. See README for the JSON shape.</summary>
        private static bool WantsLeopard(SimGameState sim)
        {
            if (sim.ConstantOverrides?.ConstantOverrides != null
                && sim.ConstantOverrides.ConstantOverrides.TryGetValue("string", out var section)
                && section.TryGetValue("StartingDropship", out var choice))
            {
                if (string.Equals(choice, "leopard", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (string.Equals(choice, "argo", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return Core.Settings?.StartShip == "Leopard";
        }
    }
}
