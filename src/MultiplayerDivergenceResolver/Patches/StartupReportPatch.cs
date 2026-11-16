using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MultiplayerDivergenceResolver.Patches;

/// <summary>
/// Logs the final character order once the game has finished loading every mod's content, so two
/// players can compare one line of godot.log to confirm they agree. Reading AllCharacters is safe
/// here; it does not freeze any pool.
/// </summary>
[HarmonyPatch(typeof(ModelIdSerializationCache), nameof(ModelIdSerializationCache.Init))]
internal static class StartupReportPatch
{
    private static void Postfix()
    {
        string order = string.Join(", ", ModelDb.AllCharacters.Select(c => c.Id.Entry));
        Log.Info($"[{ModEntry.Id}] Character order: {order}");
    }
}
