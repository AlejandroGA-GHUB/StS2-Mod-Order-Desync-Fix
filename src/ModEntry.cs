using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace ModOrderDesyncFix;

[ModInitializer(nameof(Init))]
public static class ModEntry
{
    public const string Id = "ModOrderDesyncFix";

    internal static Harmony Harmony { get; } = new("mod-order-desync-fix");

    public static void Init()
    {
        Harmony.PatchAll(typeof(ModEntry).Assembly);
        Log.Info($"[{Id}] Initialized.");
    }
}
