using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerDivergenceResolver.Patches;

// Pools append content that mods register through ModHelper.AddModelToPool in the order those calls
// happened, which is each player's mod load order. Mods that patch a pool's GenerateAllX instead
// land in Harmony patch order, which is also load order. Random rewards, transforms and "random
// card/relic/potion" effects then index into these lists, so the order has to be the same on every
// machine. The pools cache their list, so each pool is only sorted once (see GameContentOrder).

[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.AllCards), MethodType.Getter)]
internal static class CardPoolOrderPatch
{
    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<CardModel> __result)
    {
        __result = GameContentOrder.Canonicalize(__result);
    }
}

[HarmonyPatch(typeof(RelicPoolModel), nameof(RelicPoolModel.AllRelics), MethodType.Getter)]
internal static class RelicPoolOrderPatch
{
    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<RelicModel> __result)
    {
        __result = GameContentOrder.Canonicalize(__result);
    }
}

[HarmonyPatch(typeof(PotionPoolModel), nameof(PotionPoolModel.AllPotions), MethodType.Getter)]
internal static class PotionPoolOrderPatch
{
    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<PotionModel> __result)
    {
        __result = GameContentOrder.Canonicalize(__result);
    }
}
