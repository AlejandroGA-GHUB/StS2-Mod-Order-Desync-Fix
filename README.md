# Mod Order Desync Fix

A Slay the Spire 2 mod that fixes a class of **multiplayer state divergence** ("desync") that
happens when players have the same mods installed but **loaded in a different order**.

**Every player in the lobby needs it.** It is marked as gameplay-affecting, so the game enforces that.

**[Get it on the Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3841017147)**

## The problem

Cards like **Splash** and **The Lovers** give you a random card from *another character's* pool.
To do that, the game lines up every character's cards in one list and picks by position using a
shared random roll.

Modded characters add themselves to that character list one mod at a time, and they do it in each
player's own **mod load order** (your mod list order, or Steam subscription order). If two players
load, say, Watcher and a Chaos Zero Nightmare character in a different order, their lists are
ordered differently:

```
Host:   Ironclad … Fool | Watcher | Nine | Olga | Tiphera
Client: Ironclad … Fool | Nine | Olga | Tiphera | Watcher
```

The same roll then lands on a Watcher card for one player and a Nine card for the other, and the
game disconnects with a state divergence. Rolls that land on a vanilla card are fine, because
vanilla characters always come first. That is why it seems to happen "only when picking modded
cards".

The game's lobby check doesn't catch this: it compares *which* content everyone has, not the order.

The same issue applies to modded cards, relics and potions added to shared pools, and to modded
events and orbs.

## What this mod does

1. **One order for everyone.** Lists the game picks from at random are put in a fixed order:
   vanilla content first, unchanged, then modded content sorted by ID. Covered:
   - the character list (`ModelDb.AllCharacters`), and with it every per-character card, relic and potion list;
   - each card, relic and potion pool's contents;
   - shared card/relic/potion pools, shared events and ancients, event encounters, and orbs.
2. **Lobby guard.** The order of every gameplay list (including act events and encounters, which
   this mod does not reorder) is folded into the content hash the lobby already compares. If two
   players would still diverge, they are refused when joining instead of desyncing mid-run.
   Content from cosmetic-only mods is left out, so a skin one player lacks never blocks a join.

Vanilla behavior is unchanged: base-game content keeps its order, and with no modded content in
a list, nothing changes.

Tested on game version v0.113.1 in co-op with players whose mod load orders differ: Splash and
The Lovers no longer cause a divergence when they hand out modded cards.

## Install

[Subscribe on the Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3841017147)
(the game installs and updates it for you), or install it by hand:

1. Extract `ModOrderDesyncFix-<version>.zip` into `<Slay the Spire 2>\mods\`, so you end up with
   `mods\ModOrderDesyncFix\ModOrderDesyncFix.dll` and `ModOrderDesyncFix.json`.
   (The usual location is `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\`;
   create the `mods` folder if it does not exist.)
2. Start the game. New mods are enabled by default.

Every player in the lobby must install the same version.

To confirm it loaded, look in
`%APPDATA%\SlayTheSpire2\logs\godot.log` for:

```
[ModOrderDesyncFix] Character order: IRONCLAD, SILENT, …
```

Both players should see the same line.

### If you are refused with a version / model ID hash mismatch

Your mods are the same but some content is still in a different order. Each player's log has a
block starting `Content order fingerprint`. Compare it between the two logs; the first line that
differs names the list that is out of order (e.g. `act-events:ACT.…`). Please
[open an issue](https://github.com/AlejandroGA-GHUB/StS2-Mod-Order-Desync-Fix/issues) with both
blocks.

## Reporting a desync

If you still get a state divergence with this mod installed, please
[open an issue](https://github.com/AlejandroGA-GHUB/StS2-Mod-Order-Desync-Fix/issues) with both
players' `godot.log` from `%APPDATA%\SlayTheSpire2\logs\` and the card or action that triggered it.
Divergences can have causes unrelated to load order, so the logs are what tell them apart.

## Build

Requires the .NET 9 SDK (or newer) and a local install of the game.

```
dotnet build -c Release
dotnet test
```

A Release build also writes the shareable `dist\ModOrderDesyncFix-<version>.zip`.

The game install path defaults to the standard Steam location. Override it with
`-p:Sts2Dir="D:\SteamLibrary\steamapps\common\Slay the Spire 2"`. Add `-p:DeployToGame=true` to
copy the built mod into the game's `mods` folder.

One test runs Harmony exactly as the game does and needs the **.NET 9 runtime**; on newer
runtimes it is reported as skipped.

## Compatibility

- Works alongside other mods without configuration. Its patches run after every other mod's
  patch on the same lists, regardless of load order.
- Does not change saves. Adding it mid-run is safe for that run's sync, though random picks from
  modded content will differ from what they would have been without it.
- Requires game version 0.113.0 or newer.

## License

[MIT](LICENSE).
