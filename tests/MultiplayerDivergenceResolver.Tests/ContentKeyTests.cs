namespace MultiplayerDivergenceResolver.Tests;

public class ContentKeyTests
{
    private static ContentKey Key(
        string entry,
        string? modId = "SomeMod",
        bool gameplay = true,
        string category = "CHARACTER",
        string type = "Mod.Type",
        string assembly = "Mod, Version=1.0.0.0") =>
        new(gameplay, category, entry, modId, type, assembly);

    [Fact]
    public void Gameplay_mod_content_sorts_before_non_gameplay_content()
    {
        Assert.True(Key("ZZZ", gameplay: true).CompareTo(Key("AAA", gameplay: false)) < 0);
    }

    [Fact]
    public void Then_by_category_and_entry_ordinally()
    {
        Assert.True(Key("NINE").CompareTo(Key("WATCHER")) < 0);
        Assert.True(Key("Z", category: "CARD").CompareTo(Key("A", category: "CHARACTER")) < 0);
        // Ordinal, not culture-aware: upper case sorts before lower case.
        Assert.True(Key("Zeta").CompareTo(Key("alpha")) < 0);
    }

    [Fact]
    public void Same_id_breaks_ties_by_base_game_then_mod_id()
    {
        Assert.True(Key("X", modId: null).CompareTo(Key("X", modId: "A")) < 0);
        Assert.True(Key("X", modId: "A").CompareTo(Key("X", modId: "B")) < 0);
    }

    [Fact]
    public void Same_id_and_mod_breaks_ties_by_type_then_assembly()
    {
        Assert.True(Key("X", type: "A.T").CompareTo(Key("X", type: "B.T")) < 0);
        Assert.True(Key("X", assembly: "A").CompareTo(Key("X", assembly: "B")) < 0);
    }

    [Fact]
    public void Equal_keys_compare_equal()
    {
        Assert.Equal(0, Key("X").CompareTo(Key("X")));
    }
}
