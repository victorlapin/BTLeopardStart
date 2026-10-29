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
   event or mod, and every save load (the game re-asserts the persisted ship
   there, so stale tags self-heal) — re-syncs the tag to the actual ship.
   Keeps the system extensible for a potential future `current_ship_union`.
3. **Engineering is closed on the Leopard — natively, with zero overrides.**
   Room visibility is driven by the `DropshipLocation` flag masks in the game
   code: `DS_LEOPARD = 119` does not include the `ENGINEERING` bit, so the
   left-nav button is hidden and the ship upgrade screen is unreachable while
   `CurDropship == Leopard`. All vanilla `ShipModuleUpgradeDef`s stay
   untouched; their Argo-flavored texts are only ever shown on the Argo, where
   they are correct.

**Earning the Argo is built in**: while on the Leopard, a recurring event
offers a stripped old Explorer-class hull for sale (¢2,000,000 — the option
is hidden while funds are short; declining costs nothing and the offer
returns). The offer starts once the company has won 8 missions and only
comes up while **docked** — the event carries `MissionsComplete > 8` and
`Travel == 0` comparisons, so a two-million-C-Bill deal is never pitched
mid-jump. Buying it calls the native `System_SetDropship` action — the tag
sync switches the ship, and engineering/upgrades open on the Argo. Any other
content that switches the ship through the same native action works too.

`FlashpointIds` (optional): flashpoints listed there are placed in the
eligible system **nearest to the player** when they generate — vanilla picks
a random eligible system from the whole map with no distance filter, which on
large community maps (thousands of systems) can spawn content dozens of jumps
away.

Compatibility with **IRTweaks** `StreamlinedMainMenu`: its left-nav rework
posts after vanilla gating (does not reactivate hidden buttons) and its new
Store/CMD Staff/Memorial buttons use room IDs inside the Leopard mask, so the
upgrade screen stays unreachable. Note a separate, ship-agnostic IRTweaks bug:
with default (non-instant) camera transitions the CMD Staff flyouts do not
route — clicking any crew member (Yang, Sumire, Farah...) lands you in the
Command Center with Darius, and no conversation starts (its queue branch
re-queues the menu but never redirects navigation). Unrelated to this mod;
fixable locally in IRTweaks.

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
    "fp_ExampleChain1"         // the player (never the current system); empty
  ]                            // = vanilla placement (random system map-wide)
}
```

`StartShip` is only the **default**: a pack may expose a per-career choice in
the career setup screen (see below), and that selection wins for the career
being created.

## Integrating into a modpack

The `current_ship_*` company tags are a public integration surface — and there
is integration work this mod deliberately does **not** do for you: your event
mods know nothing about the Leopard phase and will happily fire with texts
that presume the Argo. When wiring this mod into a modpack, audit your events:

1. **Scan** your event mods for Argo references (search texts for `\bargo\b`,
   case-insensitive; watch out for false positives inside "cargo"). Check
   event bodies, option details and result texts — and remember that options
   hidden behind upgrade requirements are invisible on the Leopard, so Argo
   mentions that live *only* there need no work.
2. **Check self-gating first.** Many Argo-facility events already require an
   `argo_*` upgrade tag in `RequirementTags` — those upgrades can never exist
   on the Leopard, so the events never fire. Leave them alone. This applies to
   third-party mods too: verify before editing anything.
3. **Gate functionally Argo-dependent events** (premise is an Argo facility —
   hydroponics, lounge, library, pool) by adding `current_ship_leopard` to the
   requirements. Scope matters: company-scope events take it in the top-level
   `ExclusionTags`; mechwarrior-scope events need a Company-scope
   `AdditionalRequirements` entry, because their top-level block checks the
   pilot's tags, not company tags.
4. **Reword cosmetic mentions** ship-neutral ("the ship" instead of "the
   Argo") where the event's premise is location-independent — such wording
   stays correct on both ships.
5. **Flashpoint chains**: put their IDs into `FlashpointIds` for placement,
   empty their `LocationRequirements` (placement is this mod's job — vanilla
   still filters by them), and audit their forced events the same way. Note
   that mods porting the story campaign usually presume the player already
   owns the Argo — gate such chains behind `current_ship_argo` at or before
   their first Argo reference.

Engine limitation to keep in mind: the ship name cannot be a variable, so
events that can only fire post-Argo may keep their "Argo" wording verbatim —
it is correct there.

### Offering the choice in the career setup screen

Instead of fixing the ship in `settings.json`, a pack can let each new career
choose its ship right in the career difficulty screen. Add a setting to
`StreamingAssets/data/constants/CareerDifficultySettings.json`:

```json
{
  "ID": "diff_startingShip",
  "Name": "Starting Dropship",
  "TelemetryEventName": "",
  "UIOrder": 26,
  "Tooltip": "Select the DropShip your company starts on.",
  "Enabled": true,
  "Visible": true,
  "Toggle": false,
  "StartOnly": true,
  "DefaultIndex": 0,
  "Options": [
    {
      "ID": "diff_ship_leopard",
      "Name": "Leopard",
      "TelemetryEventDesc": "",
      "DifficultyValue": 0,
      "CareerScoreModifier": 0,
      "DifficultyConstants": [
        {
          "ConstantType": "string",
          "ConstantName": "StartingDropship",
          "ConstantValue": "leopard"
        }
      ]
    },
    {
      "ID": "diff_ship_argo",
      "Name": "Argo",
      "TelemetryEventDesc": "",
      "DifficultyValue": 0,
      "CareerScoreModifier": 0,
      "DifficultyConstants": [
        {
          "ConstantType": "string",
          "ConstantName": "StartingDropship",
          "ConstantValue": "argo"
        }
      ]
    }
  ]
}
```

The game parks the selected option's `DifficultyConstants` in
`SimGameState.ConstantOverrides` and saves them with the career, so the
choice survives save/load natively. On a fresh career this mod reads
`StartingDropship` from there: `leopard` runs the ship switch, `argo` leaves
the vanilla Argo start (and the built-in purchase event correctly never
fires). Any other value — or no entry at all — falls back to the mod's
`StartShip` setting, so standalone installs keep working unchanged.

`ConstantType: "string"` is deliberate: it matches no real `SimGameConstants`
section, so the value is pure data for this mod (the same trick vanilla-style
difficulty lists use for their non-constant toggles).

## Known issues

None currently. The historical ones are resolved: the flashpoint conversation
glitch (see roadmap) and Argo-flavored event texts (audit approach documented
above; the ship name cannot be an engine variable, so a few cosmetic "Argo"
mentions remain only in events that fire post-Argo anyway).

## Roadmap

### v0.1 — release
- [x] Core patch: start ship + mutually exclusive `current_ship_*` tags
- [x] Engineering natively closed on the Leopard (code audit + field check)
- [x] Tag sync on external ship switches (see How it works)
- [x] Flashpoint placement via `FlashpointIds` — a prefix on the `Flashpoint`
      constructor (the first point where the def is chosen while the system is
      still swappable); eligibility reuses the def's `LocationRequirements`
      plus the no-two-flashpoints-per-system rule; the player's current system
      is never chosen. Field-verified
- [x] Flashpoint conversation glitch: fixed and field-verified. Three layers,
      all Leopard-only: (1) the CONFERENCE room is not in the Leopard's room
      mask — entering it broke the scene (planet graphics over rooms);
      conversation entry is redirected to the Command Center via
      `RoomManager.ChangeRoom` + `CameraController.SetScene` (ChangeRoom alone
      does not move the camera); (2) conversation exits (a raw circular
      `Conference`, `Ship`/`Argo` as substituted by ExtendedConversations for
      invalid locations, or vanilla's `INVALID_UNSET` fallback) all landed on
      the SHIP exterior view — now redirected to the Command Center; (3)
      speaker-focus camera moves during conversations (the cast's camera
      anchors only exist in the Argo conference set) are skipped. Argo
      behavior untouched

### v0.2.1
- [x] Career-setup ship choice: packs can expose a "Starting Dropship" row in
      the career difficulty screen (see *Integrating into a modpack*); the
      selection overrides `StartShip` for that career

### v0.2.2
- [x] Argo purchase gate lowered to 8 won missions (was 10): repair throughput
      without ship upgrades is a fixed clock, not player skill — ten missions of
      Leopard-phase queue waiting dragged in the field
- [x] The purchase offer now also requires the company to be docked
      (`Travel == 0`) — never pitched mid-jump

### Backlog
- [ ] Union dropship support (`current_ship_union`; shares Argo visuals)
- [ ] IRTweaks fixes: the CMD Staff flyout routing bug above (queue branch
      needs a ChangeRoom call); and if instant room transitions are ever
      enabled, the Farah flyout would force the off-mask Engineering room on
      the Leopard and needs a guard

## Acknowledgments

- [Career Leopard Start](https://www.nexusmods.com/battletech/mods/518) by
  dogfisc (2020) — the original Leopard-start mod this project used as its
  reference: the feasibility proof, the Argo-recovery flashpoint chain that
  served as the transition until v0.2, and the event blocklist that seeded
  the event audit.

## License

MIT
