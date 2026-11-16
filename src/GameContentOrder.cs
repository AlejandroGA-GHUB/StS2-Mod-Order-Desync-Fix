using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace ModOrderDesyncFix;

/// <summary>
/// Applies <see cref="CanonicalOrder"/> to game models, ordering modded models by
/// <see cref="ContentKey"/>.
/// </summary>
internal static class GameContentOrder
{
    private static readonly Assembly BaseGameAssembly = typeof(AbstractModel).Assembly;

    private static readonly IComparer<AbstractModel> ModdedOrder =
        Comparer<AbstractModel>.Create((a, b) => KeyFor(a).CompareTo(KeyFor(b)));

    // Results for list instances the game keeps cached (pool card lists and the like), so a getter
    // that hands back the same instance every call is only sorted once. Keys are held weakly.
    private static readonly ConditionalWeakTable<object, object> Cache = new();

    public static IEnumerable<T> Canonicalize<T>(IEnumerable<T>? source) where T : AbstractModel
    {
        if (source is null)
        {
            return Array.Empty<T>();
        }

        if (Cache.TryGetValue(source, out object? cached))
        {
            return (IEnumerable<T>)cached;
        }

        IEnumerable<T> result = source;
        if (CanonicalOrder.HasModded(source, IsBaseGame))
        {
            result = CanonicalOrder.Apply<AbstractModel>(source, IsBaseGame, ModdedOrder).Cast<T>().ToArray();
        }

        // Before AssemblyInfo is ready, keys are built without mod ids (see ModIdFor), so an order
        // computed then could differ from the final one. Only remember orders computed after it.
        if (AssemblyInfo.ModMap is not null)
        {
            Cache.AddOrUpdate(source, result);
        }

        return result;
    }

    public static bool IsBaseGame(AbstractModel model) => model.GetType().Assembly == BaseGameAssembly;

    private static ContentKey KeyFor(AbstractModel model)
    {
        Type type = model.GetType();
        Mod? mod = ModFor(type);
        return new ContentKey(
            AffectsGameplay: mod?.manifest?.affectsGameplay ?? true,
            Category: model.Id.Category,
            Entry: model.Id.Entry,
            ModId: mod?.manifest?.id,
            TypeFullName: type.FullName ?? type.Name,
            AssemblyFullName: type.Assembly.FullName ?? string.Empty);
    }

    private static Mod? ModFor(Type type)
    {
        // AssemblyInfo is only initialized after every mod has loaded, and throws before that.
        // Without it the key still orders by id, type name and assembly name, which are stable.
        if (AssemblyInfo.ModMap is null)
        {
            return null;
        }

        return AssemblyInfo.ModForType(type, out _);
    }
}
