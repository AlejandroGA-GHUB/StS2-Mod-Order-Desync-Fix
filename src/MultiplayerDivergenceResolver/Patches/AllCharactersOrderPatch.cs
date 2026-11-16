using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerDivergenceResolver.Patches;

/// <summary>
/// Character mods append themselves to <see cref="ModelDb.AllCharacters"/> with Harmony postfixes,
/// so the order of modded characters follows whichever mod registered its postfix first, which is
/// each player's own mod load order. Every "card from another character" effect flattens the
/// character card pools in this order and picks by index, so two players with the same mods in a
/// different order roll different cards.
///
/// Example seen in the wild: Watcher appends itself at its own init, while CznCore appends the CZN
/// characters when the first CZN character mod registers. Host loaded Watcher first, client loaded
/// a CZN character first, and The Lovers gave the host WATCHER_CRUSH_JOINTS but the client
/// NINE_INSTINCT from the same roll.
///
/// The card, relic and potion pool lists ModelDb derives from AllCharacters inherit this order.
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
internal static class AllCharactersOrderPatch
{
    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        __result = GameContentOrder.Canonicalize(__result);
    }
}
