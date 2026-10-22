using System.Linq;
using BattleTech;
using HarmonyLib;

namespace BTLeopardStart.Patches
{
    /// <summary>
    /// Places selected flashpoints (the Argo-recovery chain) on the eligible system
    /// nearest to the player, instead of a random eligible system anywhere on the
    /// map (vanilla picks from all systems with no distance filter — on a
    /// full-Inner-Sphere map that means the chain can spawn dozens of jumps away).
    ///
    /// Hooked as a prefix on the Flashpoint constructor because that is the first
    /// point where the def is already chosen while the system argument can still be
    /// swapped before anything consumes it. A prefix on GenerateFlashpoint would
    /// only see defs passed explicitly (the initial StartingFlashpoint batch);
    /// TA2/TA3 are drawn from the weighted pool inside that method.
    ///
    /// Eligibility mirrors the game's own checks: the system must satisfy the def's
    /// LocationRequirements and must not already host an available flashpoint.
    /// Employer/target come from the def, not from the system owner, so swapping
    /// the system has no mechanical side effects.
    /// </summary>
    [HarmonyPatch(typeof(Flashpoint), MethodType.Constructor,
        new[] { typeof(FlashpointDef), typeof(SimGameState), typeof(StarSystem), typeof(FactionValue) })]
    public static class Flashpoint_Placement
    {
        public static void Prefix(FlashpointDef def, SimGameState state, ref StarSystem starSystem)
        {
            var ids = Core.Settings?.FlashpointIds;
            if (ids == null || ids.Count == 0 || def == null || state == null || starSystem == null)
            {
                return;
            }
            if (!ids.Contains(def.Description.Id))
            {
                return;
            }
            var origin = state.CurSystem;
            if (origin == null)
            {
                return;
            }

            StarSystem best = null;
            var bestDist = float.MaxValue;
            foreach (var sys in state.StarSystems)
            {
                if (sys == null || sys == starSystem)
                {
                    continue;
                }
                if (state.AvailableFlashpoints.Any(fp => fp != null && fp.CurSystem != null && fp.CurSystem.ID == sys.ID))
                {
                    continue;
                }
                if (!SimGameState.MeetsRequirements(def.LocationRequirements, sys.Tags, sys.Stats, null))
                {
                    continue;
                }
                var dist = DistanceSq(origin.Position, sys.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = sys;
                }
            }

            if (best == null)
            {
                return; // no eligible system (should not happen) — keep the vanilla pick
            }

            starSystem = best;
            Core.Log($"flashpoint {def.Description.Id} placed near player at {best.Name} ({best.ID})");
        }

        private static float DistanceSq(FakeVector3 a, FakeVector3 b)
        {
            var dx = a.x - b.x;
            var dy = a.y - b.y;
            var dz = a.z - b.z;
            return dx * dx + dy * dy + dz * dz;
        }
    }
}
