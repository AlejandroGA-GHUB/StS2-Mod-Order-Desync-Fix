using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerDivergenceResolver.Patches;

// The ModelDb lists that are not tied to a character, which mods extend with postfixes the same
// way they extend AllCharacters: shared card/relic/potion pools, shared events and ancients, the
// encounters events can start, and orbs. Several are private; they are patched by name.
//
// Acts and the per-act lists are deliberately NOT touched: act order is meaningful (act 1, 2, 3).

[HarmonyPatch]
internal static class SharedCardPoolsOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllSharedCardPools));

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<CardPoolModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}

[HarmonyPatch]
internal static class SharedRelicPoolsOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), "AllSharedRelicPools");

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<RelicPoolModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}

[HarmonyPatch]
internal static class SharedPotionPoolsOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), "AllSharedPotionPools");

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<PotionPoolModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}

[HarmonyPatch]
internal static class SharedEventsOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllSharedEvents));

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<EventModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}

[HarmonyPatch]
internal static class SharedAncientsOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllSharedAncients));

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<AncientEventModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}

[HarmonyPatch]
internal static class EventEncountersOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.EventEncounters));

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<EncounterModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}

[HarmonyPatch]
internal static class OrbsOrderPatch
{
    private static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.Orbs));

    [HarmonyPriority(PatchOrder.AfterEveryone)]
    private static void Postfix(ref IEnumerable<OrbModel> __result) => __result = GameContentOrder.Canonicalize(__result);
}
