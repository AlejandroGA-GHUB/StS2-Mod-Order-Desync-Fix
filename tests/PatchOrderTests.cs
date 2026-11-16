using System.Runtime.CompilerServices;
using HarmonyLib;
using ModOrderDesyncFix.Patches;

namespace ModOrderDesyncFix.Tests;

/// <summary>
/// Our ordering postfix only works if it runs after every other mod's postfix, including mods that
/// patch the same getter AFTER we do (CznCore registers its AllCharacters postfix lazily, when the
/// first CZN character mod loads). This checks that with the Harmony build the game ships.
/// </summary>
public class PatchOrderTests
{
    public static class Target
    {
        public static List<string> Calls { get; } = new();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string Value() => "original";
    }

    private static void OurPostfix() => Target.Calls.Add("ours");

    private static void OtherPostfixDefault() => Target.Calls.Add("other-normal");

    private static void OtherPostfixLast() => Target.Calls.Add("other-last");

    [GameRuntimeFact]
    public void Our_postfix_runs_after_postfixes_registered_later()
    {
        var harmony = new Harmony("test.patch-order");
        try
        {
            var original = AccessTools.Method(typeof(Target), nameof(Target.Value));

            // Ours first, as when this mod happens to load before the others.
            harmony.Patch(original, postfix: new HarmonyMethod(typeof(PatchOrderTests), nameof(OurPostfix))
            {
                priority = PatchOrder.AfterEveryone,
            });
            new Harmony("test.other-normal").Patch(original,
                postfix: new HarmonyMethod(typeof(PatchOrderTests), nameof(OtherPostfixDefault)));
            new Harmony("test.other-last").Patch(original,
                postfix: new HarmonyMethod(typeof(PatchOrderTests), nameof(OtherPostfixLast))
                {
                    priority = Priority.Last,
                });

            Target.Calls.Clear();
            Target.Value();

            Assert.Equal(["other-normal", "other-last", "ours"], Target.Calls);
        }
        finally
        {
            foreach (string id in new[] { "test.patch-order", "test.other-normal", "test.other-last" })
            {
                new Harmony(id).UnpatchAll(id);
            }
        }
    }
}
