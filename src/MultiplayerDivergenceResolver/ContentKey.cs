namespace MultiplayerDivergenceResolver;

/// <summary>
/// The sort key for one modded model. Compares the same way the game's own
/// <c>ContentSorter&lt;ModelId&gt;</c> does when it assigns network IDs, so content that the game
/// already treats as deterministically ordered sorts the same way here:
///  1. content from gameplay-affecting mods before content from non-gameplay mods;
///  2. by model id ("CATEGORY.ENTRY"), ordinal;
///  3. base game before mods, then by mod id, ordinal;
///  4. by type full name, then assembly full name, ordinal.
///
/// The game compares <c>ModelId</c> with its own <c>CompareTo</c>; this compares the category and
/// entry strings ordinally, which is equivalent for ordering purposes and keeps this file free of
/// game dependencies so it can be unit-tested.
/// </summary>
internal readonly record struct ContentKey(
    bool AffectsGameplay,
    string Category,
    string Entry,
    string? ModId,
    string TypeFullName,
    string AssemblyFullName) : IComparable<ContentKey>
{
    public int CompareTo(ContentKey other)
    {
        // true sorts first.
        int c = other.AffectsGameplay.CompareTo(AffectsGameplay);
        if (c != 0) return c;

        c = string.CompareOrdinal(Category, other.Category);
        if (c != 0) return c;

        c = string.CompareOrdinal(Entry, other.Entry);
        if (c != 0) return c;

        if (ModId is null != other.ModId is null)
        {
            return ModId is null ? -1 : 1;
        }

        c = string.CompareOrdinal(ModId, other.ModId);
        if (c != 0) return c;

        c = string.CompareOrdinal(TypeFullName, other.TypeFullName);
        if (c != 0) return c;

        return string.CompareOrdinal(AssemblyFullName, other.AssemblyFullName);
    }
}
