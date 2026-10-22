using BattleTech;
using HarmonyLib;

namespace BTLeopardStart.Patches
{
    /// <summary>
    /// Skips speaker-focus camera moves during conversations on the Leopard.
    ///
    /// Group conversations call TransitionCamera(character, ...) to frame each
    /// speaker as they talk. The lookup walks the current ship's room prefabs
    /// for a camera anchor assigned to that character — the cast's anchors only
    /// exist in the Argo's conference room set, so on the Leopard every
    /// transition fails with "Couldn't find the room!" (error spam and a stuck
    /// camera). The conversation overlay renders over the current room anyway,
    /// so skipping the move entirely is the correct Leopard behavior.
    /// </summary>
    [HarmonyPatch(typeof(SimGameCameraController), nameof(SimGameCameraController.TransitionCamera),
        new[] { typeof(SimGameState.SimGameCharacterType), typeof(bool) })]
    public static class SimGameCameraController_SpeakerFocus
    {
        public static bool Prefix()
        {
            var sim = UnityGameInstance.BattleTechGame?.Simulation;
            if (sim == null || sim.CurDropship != DropshipType.Leopard)
            {
                return true;
            }
            return false; // skip native: no character anchors exist on the Leopard
        }
    }
}
