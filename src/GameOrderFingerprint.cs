using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace ModOrderDesyncFix;

/// <summary>
/// Builds the <see cref="OrderFingerprint"/> sections from the live game content: every list that
/// gameplay picks from by index. Content from mods that declare they do not affect gameplay is left
/// out, the same way the game leaves it out of its own content hash, so a cosmetic mod one player
/// lacks never causes a refusal.
///
/// Act-specific lists are included even though this mod does not reorder them: if they differ
/// between players the run would diverge anyway, and a refusal in the lobby is the better outcome.
/// </summary>
internal static class GameOrderFingerprint
{
    private static readonly object Gate = new();
    private static IReadOnlyList<OrderFingerprint.Section>? _sections;

    /// <summary>
    /// Computed once, on first use. That is the first time a lobby is opened or joined, long after
    /// every mod has registered its content, so reading the pools here cannot freeze them early.
    /// </summary>
    public static IReadOnlyList<OrderFingerprint.Section> Sections
    {
        get
        {
            lock (Gate)
            {
                if (_sections is null)
                {
                    _sections = Build();
                    LogSections(_sections);
                }

                return _sections;
            }
        }
    }

    private static List<OrderFingerprint.Section> Build()
    {
        var sections = new List<OrderFingerprint.Section>
        {
            Section("characters", ModelDb.AllCharacters),
        };

        foreach (CardPoolModel pool in ModelDb.AllCardPools.Where(AffectsGameplay))
        {
            sections.Add(Section($"cards:{pool.Id}", pool.AllCards));
        }

        foreach (RelicPoolModel pool in ModelDb.AllRelicPools.Where(AffectsGameplay))
        {
            sections.Add(Section($"relics:{pool.Id}", pool.AllRelics));
        }

        foreach (PotionPoolModel pool in ModelDb.AllPotionPools.Where(AffectsGameplay))
        {
            sections.Add(Section($"potions:{pool.Id}", pool.AllPotions));
        }

        sections.Add(Section("shared-events", ModelDb.AllSharedEvents));
        sections.Add(Section("shared-ancients", ModelDb.AllSharedAncients));
        sections.Add(Section("event-encounters", ModelDb.EventEncounters));
        sections.Add(Section("orbs", ModelDb.Orbs));

        foreach (ActModel act in ModelDb.Acts.Where(AffectsGameplay))
        {
            sections.Add(Section($"act-events:{act.Id}", act.AllEvents));
            sections.Add(Section($"act-encounters:{act.Id}", act.AllEncounters));
            sections.Add(Section($"act-ancients:{act.Id}", act.AllAncients));
        }

        return sections;
    }

    private static OrderFingerprint.Section Section<T>(string name, IEnumerable<T>? models) where T : AbstractModel =>
        OrderFingerprint.HashSection(
            name,
            (models ?? Enumerable.Empty<T>()).Where(m => m is not null && AffectsGameplay(m)).Select(m => m.Id.ToString()));

    private static bool AffectsGameplay(AbstractModel model)
    {
        if (GameContentOrder.IsBaseGame(model) || AssemblyInfo.ModMap is null)
        {
            return true;
        }

        return AssemblyInfo.ModForType(model.GetType(), out _)?.manifest?.affectsGameplay ?? true;
    }

    private static void LogSections(IReadOnlyList<OrderFingerprint.Section> sections)
    {
        Log.Info($"[{ModEntry.Id}] Content order fingerprint ({sections.Count} lists). If two players with the same mods are refused " +
                 "with a model ID hash mismatch, compare these lines between their logs; the first line that differs is the list in a different order.");
        foreach (OrderFingerprint.Section section in sections)
        {
            Log.Info($"[{ModEntry.Id}]   {section.Name}: {section.Hash:x8} ({section.Count})");
        }
    }
}
