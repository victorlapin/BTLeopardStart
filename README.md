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

The Argo transition itself is **not part of this mod** — it ships as the
companion mod **"Flashpoint - Recover The Argo"** (from dogfisc's 2020
"Career Leopard Start"), retuned for this pack: three linked flashpoints
(difficulty 2/4/5) that walk the crew through finding and recovering the
Argo, lance requests in pack vocabulary, no location pinning. That mod
`DependsOn` this one: the `FlashpointIds` placement below keeps the chain
within one jump of the player instead of anywhere on the 2888-system map.
No timers, no scripted purchase.

Verified compatible with **IRTweaks** `StreamlinedMainMenu`: its left-nav
rework posts after vanilla gating (does not reactivate hidden buttons), its
new Store/CMD Staff/Memorial buttons use room IDs inside the Leopard mask,
and the Farah flyout opens a conversation, not the upgrade screen.

## Requirements

- ModTek
- Career mode (story mode unaffected)

## Settings (`settings.json`)

Read from the mod folder at load (ModTek 4.x does not merge `settings.json`
into the entry-point settings, so the mod reads the file itself).

```jsonc
{
  "StartShip": "Leopard",      // "Leopard" or "Argo" (Argo = vanilla behavior)
  "ShipTagPrefix": "current_ship",
  "FlashpointIds": [           // flashpoints placed in the system NEAREST to
    "fp_TA1_DiggingintheDirt", // the player (never the current system) when
    "fp_TA2_ResearchAssistant",// they generate; empty = vanilla placement
    "fp_TA3_RecovertheArgo"    // (random eligible system map-wide)
  ]
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
- [x] Verify engineering is natively closed on the Leopard (code audit + field
      check done)
- [x] Integrate the transition: dogfisc's flashpoint chain added to the pack
      as "Flashpoint - Recover The Argo"; retuned difficulty 2/4/5, lance
      requests rewritten to pack vocabulary (the `dynamic_difficulty` tags had
      zero matches in the lance pool), `LocationRequirements` emptied
      (placement is this mod's job), dead `Repeatable` field dropped
- [x] Flashpoint placement: flashpoints listed in `FlashpointIds` spawn on the
      eligible system nearest to the player — never the player's current
      system — instead of a random eligible system from the whole map.
      Implemented as a prefix on the `Flashpoint` constructor (the first point
      where the def is chosen while the system is still swappable); eligibility
      reuses the def's `LocationRequirements` plus the no-two-flashpoints-per-
      system rule. Field-verified on game 1.9.1
- [x] Tag sync on external ship switches: postfix on `SetSimShip` so that any
      `System_SetDropship` action (the FPs use it) keeps `current_ship_*`
      consistent — without this the tags go stale after the FP chain
- [ ] Field test, remaining: TA1 combat (lance composition, difficulty feel);
      FP conversation graphics glitch; Axylus finale → Argo transition;
      save/load round-trip after the transition. Verified so far: Leopard
      start + tags, engineering unreachable, flashpoint placement
- [ ] Flashpoint glitch: reproduce, root-cause, patch or document

### v0.2 — content
- [x] Event audit (BTVL Vanilla Events, direct edits): 27 events from the
      2020 mod's blocklist excluded on the Leopard via `current_ship_leopard`
      (company-scope events: top-level ExclusionTags; mechwarrior-scope:
      a Company-scope AdditionalRequirements entry — the MW block checks pilot
      tags, not company tags); 7 events self-gate on ship-upgrade
      requirements; 6 events reworded to ship-neutral texts ("the ship"
      instead of "the Argo" — stays correct on both ships); 6 events clean
      (Argo only mentioned inside upgrade-gated, hidden options)
- [x] MRB / black market chain (BT Advanced Events): the chain ENTRY event
      `event_sh_gate1` is gated with `current_ship_leopard` — the epsilon
      authorization (and the black market access it grants) cannot start while
      on the Leopard. The later `event_sh_gate1_assessment` keeps its original
      Argo-referencing texts: it can only fire once the chain has started,
      which is already post-Argo, so the texts are correct
- [x] Starting MechTech/MedTech counts — decided: keep vanilla (Argo-baseline)
      constants; the Leopard plays as a "cut-down Argo" stat-wise
- [x] Arano Restoration chain (Flashpoint mod): `fp_StoryTime3` (Axylus /
      "Capture the Scheria") gated behind `current_ship_argo`; the two d2
      intro missions stay open from career start. The career port presumes the
      player already owns the Argo — the Axylus briefing itself says the
      *Scheria* is "a derelict similar to your Argo", and every later event
      casually references the Argo as the player's ship. StoryTime3 is exactly
      where those references begin, so the chain runs on the Leopard up to that
      point at appropriate difficulty and pauses until the Argo is secured

### Backlog
- [ ] Union dropship support (`current_ship_union`; shares Argo visuals)
- [ ] Farah flyout room-change patch if the field test shows it looks broken

## License

MIT
