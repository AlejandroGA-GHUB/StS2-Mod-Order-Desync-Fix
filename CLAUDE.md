# Multiplayer Divergence Resolver

Slay the Spire 2 mod (C#, Harmony) fixing multiplayer state divergence caused by players loading
the same mods in a different order. Intended to be open source and published on the Steam Workshop.
README.md explains it for users; this file is for whoever works on it next.

## Root cause (established 2026-11-15, do not re-derive)

- Mods add characters to `ModelDb.AllCharacters` with Harmony postfixes. Postfixes at equal priority
  run in registration order, which is mod load order, which is PER USER (`ModManager.SortModList`:
  user's saved mod list order, then Steam subscription order, then dependencies).
- Watcher appends itself at its own init. CznCore registers its postfix lazily, on the first
  `CznCharacterRegistry.Register<T>()` call from a CZN character mod. So Watcher vs CZN order
  differs between players with different load orders.
- `Splash` and `TheLovers` (vanilla) flatten `Owner.UnlockState.CharacterCardPools` (which is
  `AllCharacters` order) and pick by RNG index (`CardFactory.GetDistinctForCombat` /
  `GetForCombat`). Same roll, different list, different card. Divergence only surfaces when the
  chosen card is modded, since vanilla characters lead the list on every machine.
- Evidence: user's godot.log (The Lovers: host WATCHER_CRUSH_JOINTS vs client NINE_INSTINCT) and
  five RitsuLib divergence reports (all Splash) that list both peers' load orders, which differ.
- The handshake's `ModelIdSerializationCache.Hash` sorts IDs (`ContentSorter`) before hashing, so
  it is order-blind and never catches this.
- `ModHelper.ConcatModelsFromMods` appends `AddModelToPool` content in call order: same flaw for pools.

## Design

- `CanonicalOrder` (pure): base-game items keep order and lead; modded items stably sorted.
  Keeps duplicates (weighting). `ContentKey` (pure) mirrors `ContentSorter`'s comparison.
- `GameContentOrder`: applies it to models; caches per source instance only after
  `AssemblyInfo.ModMap` exists (before that, mod ids are unknown and keys differ).
- Patches use `PatchOrder.AfterEveryone` (`Priority.Last - 100`). Verified in the game's
  0Harmony 2.4.2 (`PatchInfoSerialization.PriorityComparer`): priority descending, then index;
  negative is fine.
- Handshake guard patches `PeerVersionInfo.LocalDefault`, not the `Hash` getter (auto-property,
  likely JIT-inlined). Fingerprint is computed lazily there because reading pools freezes
  `AddModelToPool`.
- Deliberately NOT reordered: acts and per-act lists (order meaningful / instance getters on
  abstract members). They are fingerprinted only.

## Working on it

- Decompile locally, never commit: `ilspycmd -p -o <scratch> "<install>\data_sts2_windows_x86_64\sts2.dll"`.
- Build references the game install (`Sts2Dir` property). Tests compile the pure files directly and
  never load sts2.dll. The Harmony test needs the .NET 9 runtime (skips otherwise).
- Init order in game: `ModManager.Initialize` -> `AssemblyInfo.Init` -> `ModelDb.Init` ->
  `ModelIdSerializationCache.Init` (`OneTimeInitialization`).
- Mod DLL must be named `<manifest id>.dll`.

## Not yet verified

- In-game run of the built mod, and a real two-player session with mismatched load orders.
