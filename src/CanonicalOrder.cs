namespace ModOrderDesyncFix;

/// <summary>
/// Puts a content list in an order that depends only on WHICH items are in it, never on the order
/// they were added in.
///
/// Why: modded content reaches lists like <c>ModelDb.AllCharacters</c> through Harmony postfixes
/// and <c>ModHelper.AddModelToPool</c>, both of which run in each player's own mod load order.
/// Effects that pick a random entry by index (Splash, The Lovers, ...) then pick different content
/// on different machines from the same RNG roll, and the run diverges.
///
/// The rule:
///  - base-game items keep their relative order and come first, so vanilla behavior is unchanged;
///  - modded items follow, stably sorted by <paramref name="moddedOrder"/>;
///  - every occurrence is kept, so a list that repeats an entry on purpose keeps its weighting.
///
/// This file has no game dependencies so it can be unit-tested on its own.
/// </summary>
internal static class CanonicalOrder
{
    /// <returns>True when <paramref name="items"/> holds at least one modded item, i.e. when its
    /// order can depend on mod load order at all.</returns>
    public static bool HasModded<T>(IEnumerable<T> items, Func<T, bool> isBaseGame)
    {
        foreach (T item in items)
        {
            if (item is not null && !isBaseGame(item))
            {
                return true;
            }
        }

        return false;
    }

    public static T[] Apply<T>(IEnumerable<T> items, Func<T, bool> isBaseGame, IComparer<T> moddedOrder)
    {
        var baseGame = new List<T>();
        var modded = new List<T>();

        foreach (T item in items)
        {
            if (item is null)
            {
                continue;
            }

            (isBaseGame(item) ? baseGame : modded).Add(item);
        }

        // OrderBy is a stable sort: items that compare equal keep their input order. Equal items
        // are the same model, so that order cannot leak into gameplay.
        return baseGame.Concat(modded.OrderBy(item => item, moddedOrder)).ToArray();
    }
}
