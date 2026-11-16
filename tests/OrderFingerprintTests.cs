namespace ModOrderDesyncFix.Tests;

public class OrderFingerprintTests
{
    [Fact]
    public void Same_ids_in_a_different_order_hash_differently()
    {
        var host = OrderFingerprint.HashSection("characters", ["CHARACTER.WATCHER", "CHARACTER.NINE"]);
        var client = OrderFingerprint.HashSection("characters", ["CHARACTER.NINE", "CHARACTER.WATCHER"]);

        Assert.NotEqual(host.Hash, client.Hash);
    }

    [Fact]
    public void Same_ids_in_the_same_order_hash_the_same()
    {
        var a = OrderFingerprint.HashSection("characters", ["CHARACTER.NINE", "CHARACTER.WATCHER"]);
        var b = OrderFingerprint.HashSection("characters", ["CHARACTER.NINE", "CHARACTER.WATCHER"]);

        Assert.Equal(a, b);
        Assert.Equal(2, a.Count);
    }

    [Fact]
    public void Id_boundaries_matter()
    {
        var a = OrderFingerprint.HashSection("s", ["AB", "C"]);
        var b = OrderFingerprint.HashSection("s", ["A", "BC"]);

        Assert.NotEqual(a.Hash, b.Hash);
    }

    [Fact]
    public void Section_name_is_part_of_the_hash()
    {
        var a = OrderFingerprint.HashSection("cards:A", ["X"]);
        var b = OrderFingerprint.HashSection("cards:B", ["X"]);

        Assert.NotEqual(a.Hash, b.Hash);
    }

    [Fact]
    public void Combined_hash_changes_with_the_game_hash_and_with_any_section()
    {
        var s1 = OrderFingerprint.HashSection("a", ["1"]);
        var s2 = OrderFingerprint.HashSection("b", ["2"]);
        var s2Reordered = OrderFingerprint.HashSection("b", ["2", "3"]);

        uint baseline = OrderFingerprint.Combine(42, [s1, s2]);

        Assert.Equal(baseline, OrderFingerprint.Combine(42, [s1, s2]));
        Assert.NotEqual(baseline, OrderFingerprint.Combine(43, [s1, s2]));
        Assert.NotEqual(baseline, OrderFingerprint.Combine(42, [s1, s2Reordered]));
        Assert.NotEqual(baseline, OrderFingerprint.Combine(42, [s2, s1]));
    }

    [Fact]
    public void Hash_is_stable_across_runs()
    {
        // Pinned so an accidental change to the algorithm, which would make two versions of this
        // mod refuse each other, shows up as a failing test.
        var section = OrderFingerprint.HashSection("characters", ["CHARACTER.IRONCLAD"]);

        Assert.Equal(StableHashOf("characters", "\n", "CHARACTER.IRONCLAD"), section.Hash);
    }

    // Plain FNV-1a over the concatenated UTF-8 text, written independently of the implementation.
    private static uint StableHashOf(params string[] parts)
    {
        uint hash = 2166136261;
        foreach (byte b in System.Text.Encoding.UTF8.GetBytes(string.Concat(parts)))
        {
            hash = (hash ^ b) * 16777619;
        }

        return hash;
    }
}
