using BattleTech;
using HarmonyLib;

namespace BTLeopardStart.Patches
{
    /// <summary>
    /// Keeps flashpoint/briefing conversations off the Leopard's missing rooms.
    ///
    /// The CONFERENCE room (128) is not in the Leopard's room mask
    /// (DS_LEOPARD = 119) — entering it leaves the camera/scene in a broken
    /// state that shows up as planet graphics over the rooms (the 2020 mod's
    /// known flashpoint glitch). Two halves:
    ///
    /// 1. Entry: while on the Leopard, skip SetConferenceRoom(true) entirely —
    ///    the conversation overlay renders over the current room, no briefing
    ///    room needed.
    /// 2. Exit: conversation exits resolve to the SHIP exterior view (vanilla's
    ///    INVALID_UNSET fallback is SetSimRoomState(SHIP); ExtendedConversations
    ///    funnels invalid/circular exits there explicitly) — redirect those to
    ///    the Command Center (Contract menu), an interior room in the mask.
    ///    Argo behavior is untouched.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.SetConferenceRoom))]
    public static class SimGameState_ConferenceExit
    {
        public static bool Prefix(SimGameState __instance, bool inRoom, ref DropshipMenuType exitOverride)
        {
            if (__instance.CurDropship != DropshipType.Leopard)
            {
                return true;
            }

            if (inRoom)
            {
                // Conversations often start from the starmap / ship exterior view —
                // without the conference room to land in, the dialogue would play
                // over the space scene. ChangeRoom alone only updates state/nav,
                // the camera is driven by SetScene.
                Core.Log("conference room entry redirected to Command Center on Leopard");
                __instance.RoomManager.ChangeRoom(DropshipLocation.CMD_CENTER);
                __instance.CameraController.SetScene(DropshipLocation.CMD_CENTER);
                return false; // skip native: never enter the off-mask conference room
            }

            var requested = exitOverride;
            Core.Log($"conference exit: requested {requested}");
            if (requested == DropshipMenuType.Ship || requested == DropshipMenuType.Argo
                || requested == DropshipMenuType.Conference)
            {
                // Conference is circular (the conversation room itself) and would
                // re-enter the off-mask room
                exitOverride = DropshipMenuType.Contract;
                Core.Log($"conference exit redirected {requested} -> Contract (Command Center)");
                return true;
            }
            // An unset exit falls through to the SHIP exterior view unless a
            // contract debrief is pending — keep the debrief path (NONE) intact
            if (exitOverride == DropshipMenuType.INVALID_UNSET
                && __instance.PendingMilestoneContract == null
                && __instance.CompletedContract == null)
            {
                exitOverride = DropshipMenuType.Contract;
                Core.Log("conference exit redirected unset -> Contract (Command Center)");
            }
            return true;
        }
    }
}
