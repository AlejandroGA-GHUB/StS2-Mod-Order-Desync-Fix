using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace MultiplayerDivergenceResolver;

[ModInitializer(nameof(Init))]
public static class ModEntry
{
    public const string Id = "MultiplayerDivergenceResolver";

    internal static Harmony Harmony { get; } = new("multiplayer-divergence-resolver");

    public static void Init()
    {
        Harmony.PatchAll(typeof(ModEntry).Assembly);
        Log.Info($"[{Id}] Initialized.");
    }
}
