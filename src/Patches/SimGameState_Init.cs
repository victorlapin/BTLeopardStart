using BattleTech;
using HarmonyLib;

namespace BTLeopardStart.Patches
{
    /// <summary>Overrides the career-mode starting dropship and sets the
    /// corresponding current_ship_* tag. Tags are mutually exclusive —
    /// starting on Leopard sets current_ship_leopard (clears others);
    /// starting on Argo sets current_ship_argo (upgrades available).
    /// Future: Union transition sets current_ship_union (mid-tier).</summary>
    [HarmonyPatch(typeof(SimGameState), "_OnFirstPlayInit")]
    public static class SimGameState_OnFirstPlayInit
    {
        public static void Postfix(SimGameState __instance, SimGameState.SimGameType gameType)
        {
            if (gameType != SimGameState.SimGameType.CAREER) return;
            if (Core.Settings == null) return;

            var prefix = Core.Settings.ShipTagPrefix; // "current_ship"

            if (Core.Settings.StartShip == "Leopard")
            {
                __instance.SetSimShip(DropshipType.Leopard);
                SetShipTag(__instance, $"{prefix}_leopard");
                Core.Log("Career started on Leopard");
            }
            else if (Core.Settings.StartShip == "Argo")
            {
                // vanilla ship, but the tag must exist or gated upgrades stay locked forever
                SetShipTag(__instance, $"{prefix}_argo");
                Core.Log("Career started on Argo (upgrades unlocked)");
            }
        }

        /// <summary>Sets the current ship tag, clearing any other ship tags
        /// (mutual exclusivity).</summary>
        private static void SetShipTag(SimGameState sim, string tag)
        {
            // clear all ship tags first
            var prefix = Core.Settings?.ShipTagPrefix ?? "current_ship";
            var toRemove = new System.Collections.Generic.List<string>();
            foreach (var t in sim.CompanyTags)
                if (t.StartsWith(prefix))
                    toRemove.Add(t);
            foreach (var t in toRemove)
                sim.CompanyTags.Remove(t);

            sim.CompanyTags.Add(tag);
        }
    }
}
