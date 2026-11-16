namespace ModOrderDesyncFix.Tests;

public class CanonicalOrderTests
{
    private sealed record Model(string Id, bool BaseGame);

    private static readonly IComparer<Model> ById =
        Comparer<Model>.Create((a, b) => string.CompareOrdinal(a.Id, b.Id));

    private static string[] Order(IEnumerable<Model> items) =>
        CanonicalOrder.Apply(items, m => m.BaseGame, ById).Select(m => m.Id).ToArray();

    private static readonly Model Ironclad = new("IRONCLAD", true);
    private static readonly Model Silent = new("SILENT", true);
    private static readonly Model Fool = new("FOOL", true);
    private static readonly Model Watcher = new("WATCHER", false);
    private static readonly Model Nine = new("NINE", false);
    private static readonly Model Olga = new("OLGA", false);
    private static readonly Model Tiphera = new("TIPHERA", false);

    [Fact]
    public void Reproduced_load_orders_from_the_divergence_report_agree()
    {
        // Host loaded Watcher before the CZN characters; the client loaded a CZN character first.
        Model[] host = [Ironclad, Silent, Fool, Watcher, Nine, Olga, Tiphera];
        Model[] client = [Ironclad, Silent, Fool, Nine, Olga, Tiphera, Watcher];

        Assert.Equal(Order(host), Order(client));
    }

    [Fact]
    public void Every_permutation_of_modded_items_gives_the_same_order()
    {
        Model[] modded = [Watcher, Nine, Olga, Tiphera];
        string[]? expected = null;

        foreach (Model[] permutation in Permutations(modded))
        {
            string[] actual = Order(new[] { Ironclad, Silent }.Concat(permutation));
            expected ??= actual;
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Base_game_items_keep_their_order_and_come_first()
    {
        string[] result = Order([Watcher, Silent, Nine, Ironclad, Fool]);

        Assert.Equal(["SILENT", "IRONCLAD", "FOOL", "NINE", "WATCHER"], result);
    }

    [Fact]
    public void A_list_without_modded_items_is_unchanged()
    {
        Model[] vanilla = [Silent, Fool, Ironclad];

        Assert.False(CanonicalOrder.HasModded(vanilla, m => m.BaseGame));
        Assert.Equal(["SILENT", "FOOL", "IRONCLAD"], Order(vanilla));
    }

    [Fact]
    public void Repeated_entries_are_kept_so_weighting_survives()
    {
        string[] result = Order([Nine, Ironclad, Nine, Watcher, Ironclad]);

        Assert.Equal(["IRONCLAD", "IRONCLAD", "NINE", "NINE", "WATCHER"], result);
    }

    [Fact]
    public void Nulls_are_dropped()
    {
        string[] result = Order([Ironclad, null!, Nine]);

        Assert.Equal(["IRONCLAD", "NINE"], result);
    }

    private static IEnumerable<T[]> Permutations<T>(T[] items)
    {
        if (items.Length <= 1)
        {
            yield return items;
            yield break;
        }

        for (int i = 0; i < items.Length; i++)
        {
            T[] rest = items.Where((_, j) => j != i).ToArray();
            foreach (T[] tail in Permutations(rest))
            {
                yield return [items[i], .. tail];
            }
        }
    }
}
