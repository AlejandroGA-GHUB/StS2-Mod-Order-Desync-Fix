using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer;

namespace ModOrderDesyncFix.Patches;

/// <summary>
/// Folds the content order fingerprint into the model ID hash the handshake already compares, so
/// players whose content order still differs (a list this mod does not reorder, or a mod that
/// bypasses the patched getters) are refused in the lobby instead of diverging mid-run.
///
/// Patched at <see cref="PeerVersionInfo.LocalDefault"/>, the one place the local handshake info is
/// built, rather than at the hash getter: that getter is a trivial auto-property the JIT may inline
/// into its callers, which would silently skip a patch on it.
/// </summary>
[HarmonyPatch(typeof(PeerVersionInfo), nameof(PeerVersionInfo.LocalDefault))]
internal static class HandshakeOrderGuardPatch
{
    private static bool _logged;

    private static void Postfix(ref PeerVersionInfo __result)
    {
        uint gameHash = __result.idDatabaseHash;
        uint combined = OrderFingerprint.Combine(gameHash, GameOrderFingerprint.Sections);
        __result.idDatabaseHash = combined;

        if (!_logged)
        {
            _logged = true;
            Log.Info($"[{ModEntry.Id}] Handshake content hash {gameHash} -> {combined} (includes content order).");
        }
    }
}
