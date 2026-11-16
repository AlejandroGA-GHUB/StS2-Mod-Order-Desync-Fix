using System.Text;

namespace ModOrderDesyncFix;

/// <summary>
/// A hash of content ORDER: the IDs of each list, in the order the game will index them.
///
/// The game's own content hash sorts IDs before hashing, so two players whose lists hold the same
/// content in a different order pass the handshake and then diverge mid-run. Folding this into the
/// hash the handshake already compares turns that into a refusal in the lobby instead.
///
/// FNV-1a 32-bit: tiny, dependency-free and identical on every machine. It only has to tell
/// "same order" from "different order", not resist tampering.
/// </summary>
internal static class OrderFingerprint
{
    private const uint OffsetBasis = 2166136261;
    private const uint Prime = 16777619;

    public readonly record struct Section(string Name, uint Hash, int Count);

    public static Section HashSection(string name, IEnumerable<string> orderedIds)
    {
        uint hash = Append(OffsetBasis, name);
        int count = 0;
        foreach (string id in orderedIds)
        {
            // Separator first, so ["AB", "C"] and ["A", "BC"] hash differently.
            hash = Append(hash, "\n");
            hash = Append(hash, id);
            count++;
        }

        return new Section(name, hash, count);
    }

    /// <summary>Combines the game's own content hash with every section hash, in order.</summary>
    public static uint Combine(uint gameHash, IEnumerable<Section> sections)
    {
        uint hash = AppendUInt(OffsetBasis, gameHash);
        foreach (Section section in sections)
        {
            hash = AppendUInt(hash, section.Hash);
        }

        return hash;
    }

    private static uint Append(uint hash, string text)
    {
        foreach (byte b in Encoding.UTF8.GetBytes(text))
        {
            hash = (hash ^ b) * Prime;
        }

        return hash;
    }

    private static uint AppendUInt(uint hash, uint value)
    {
        for (int shift = 0; shift < 32; shift += 8)
        {
            hash = (hash ^ ((value >> shift) & 0xFF)) * Prime;
        }

        return hash;
    }
}
