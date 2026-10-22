using System.Collections.Generic;
using BattleTech;
using HarmonyLib;

namespace BTLeopardStart.Patches
{
    /// <summary>
    /// Single authority for the current_ship_* company tags: every ship switch,
    /// no matter the source (native career init, a System_SetDropship action from
    /// any event or mod, the debug console, or the save-load re-assert), re-syncs
    /// the tag to match the actual ship. Events gate on these tags (e.g. the
    /// Argo-recovery flashpoint chain's follow-up content).
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.SetSimShip))]
    public static class SimGameState_SetSimShip
    {
        public static void Postfix(SimGameState __instance, DropshipType dropship)
        {
            var suffix = SuffixFor(dropship);
            if (suffix == null)
            {
                return; // transient INVALID_UNSET the game sets during fresh init
            }

            SetShipTag(__instance, Prefix + "_" + suffix);
        }

        private static string Prefix => Core.Settings?.ShipTagPrefix ?? "current_ship";

        private static string SuffixFor(DropshipType dropship)
        {
            switch (dropship)
            {
                case DropshipType.Leopard: return "leopard";
                case DropshipType.Union: return "union";
                case DropshipType.Argo: return "argo";
                default: return null;
            }
        }

        internal static void SetShipTag(SimGameState sim, string tag)
        {
            var prefix = Prefix;
            var toRemove = new List<string>();
            foreach (var existing in sim.CompanyTags)
            {
                if (existing != null && existing.StartsWith(prefix))
                {
                    toRemove.Add(existing);
                }
            }
            foreach (var stale in toRemove)
            {
                sim.CompanyTags.Remove(stale);
            }
            sim.CompanyTags.Add(tag);
        }
    }
}
