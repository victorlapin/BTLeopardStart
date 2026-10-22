# BTLeopardStart

Career mode starts on a Leopard dropship instead of the Argo. The Argo is
earned through play — until then, the company lives in a cramped Leopard with
no ship upgrades at all.

## How it works

The game's `CurDropship` (Leopard vs Argo) is cosmetic-only for gameplay
systems (mech bays, berths and upgrades are stat-driven). Everything in this
mod rides on native mechanics:

1. **Harmony postfix** on `SimGameState._OnAttachUXComplete` switches the
   career starting ship via the native `SetSimShip()`. This is the only safe
   hook: the game sets the career ship to Argo inside that same method (an
   earlier hook gets overwritten), and `SetSimShip` needs `SpaceController`,
   which does not exist until the sim scene loads (an earlier hook NREs).
2. **Tag sync** — a postfix on `SetSimShip` itself is the single authority for
   the mutually exclusive `current_ship_*` company tags (prefix configurable).
   Every code path — native init, any `System_SetDropship` action from any
   event or mod (the Argo-recovery flashpoints use it), and every save load
   (the game re-asserts the persisted ship there, so stale tags self-heal) —
   re-syncs the tag to the actual ship. Keeps the system extensible for a
   future `current_ship_union`.
3. **Engineering is closed on the Leopard — natively, with zero overrides.**
   Room visibility is driven by the `DropshipLocation` flag masks in the game
   code: `DS_LEOPARD = 119` does not include the `ENGINEERING` bit, so the
   left-nav button is hidden and the ship upgrade screen is unreachable while
   `CurDropship == Leopard`. All 35 `ShipModuleUpgradeDef`s stay untouched;
   their Argo-flavored texts are only ever shown on the Argo, where they are
   correct.

The Argo transition itself is **not part of this mod** — the plan is to reuse
the three linked flashpoints from the 2020 "Career Leopard Start" mod
(dogfisc), which walk the crew through finding and recovering the Argo. No
timers, no scripted purchase.

Verified compatible with **IRTweaks** `StreamlinedMainMenu`: its left-nav
rework posts after vanilla gating (does not reactivate hidden buttons), its
new Store/CMD Staff/Memorial buttons use room IDs inside the Leopard mask,
and the Farah flyout opens a conversation, not the upgrade screen.

## Requirements

- ModTek
- Career mode (story mode unaffected)

## Settings (`settings.json`)

```jsonc
{
  "StartShip": "Leopard",      // "Leopard" or "Argo" (Argo = vanilla behavior)
  "ShipTagPrefix": "current_ship"
}
```

## Known issues

- **Flashpoint graphics glitch** (reported by the 2020 "Career Leopard Start"
  mod): after a flashpoint conversation, Leopard room graphics may be overlaid
  by planet graphics. Cosmetic; cleared by loading a save. Reproduce, then
  patch or document. Doubly relevant now that the transition rides on
  flashpoints.
- Vanilla events freely say "Argo" in their texts. The game has no way to make
  the ship name a variable; see the roadmap for the event-audit pass.

## Roadmap

### v0.1 — release
- [x] Core patch: start ship + mutually exclusive `current_ship_*` tags
- [x] Verify engineering is natively closed on the Leopard (code audit done,
      field check pending)
- [ ] Integrate the transition: pull dogfisc's flashpoint chain (3 linked FPs
      to recover the Argo) into the pack; verify on game 1.9.1
- [x] Flashpoint placement: flashpoints listed in `FlashpointIds` (the Argo
      chain) spawn on the eligible system nearest to the player — vanilla picks
      a random eligible system from the whole map with no distance filter.
      Implemented as a prefix on the `Flashpoint` constructor (the first point
      where the def is chosen while the system is still swappable); eligibility
      reuses the def's `LocationRequirements` plus the no-two-flashpoints-per-
      system rule
- [x] Tag sync on external ship switches: postfix on `SetSimShip` so that any
      `System_SetDropship` action (the FPs use it) keeps `current_ship_*`
      consistent — without this the tags go stale after the FP chain
- [ ] Field test: new career on Leopard; tags in save; engineering
      unreachable; CMD Staff → Farah cosmetics; FP chain completes → Argo +
      engineering opens; save/load round-trip
- [ ] Flashpoint glitch: reproduce, root-cause, patch or document

### v0.2 — content
- [ ] Event audit: gate functionally Argo-dependent events behind
      `ExcludedTags: current_ship_leopard` (ShouldAppend overrides over
      BTVL Vanilla Events); cosmetic name mentions stay
- [ ] MRB assessment event gating (decision pending)
- [ ] Black market membership event gating (decision pending)
- [ ] Optional: `StartMechTechs` / `StartMedTechs` overrides for a leaner
      Leopard baseline (vanilla career constants assume the Argo; kept as-is
      by default)

### Backlog
- [ ] Union dropship support (`current_ship_union`; shares Argo visuals)
- [ ] Farah flyout room-change patch if the field test shows it looks broken

## License

MIT
