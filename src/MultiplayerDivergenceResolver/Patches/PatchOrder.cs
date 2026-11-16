using HarmonyLib;

namespace MultiplayerDivergenceResolver.Patches;

internal static class PatchOrder
{
    /// <summary>
    /// Below <see cref="Priority.Last"/>, so our postfixes run after every other mod's postfix on the
    /// same getter, including ones registered after ours. Harmony orders patches by priority first
    /// and registration order second, so priority is what makes this independent of load order.
    /// </summary>
    public const int AfterEveryone = Priority.Last - 100;
}
