using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Turbo.Primitives.Players;

/// <summary>
/// A player's id as the public API shows it: <c>&lt;prefix&gt;-&lt;32 hex&gt;</c>, as Habbo's
/// <c>hhus-1ba328d1f868433da1e68df5af10f053</c> (official API answers, 2026-10-16). Habbo's are
/// opaque and stable; here the 32 hex digits are the player id sealed in one AES block under
/// <paramref name="key"/>, so the same player always gets the same id, an id reads back to its
/// player without a table, and a made-up one does not read at all. The key keeps ids from being
/// counted through, not anything private: changing it changes every player's id.
/// </summary>
public sealed class PlayerUniqueIds(string prefix, byte[] key)
{
    // The rest of the block: an id that decrypts to anything else is not one of ours.
    private static readonly byte[] MARKER = Encoding.ASCII.GetBytes("turbo.player");

    private readonly string _prefix = prefix + "-";

    public string Format(int playerId)
    {
        Span<byte> block = stackalloc byte[16];

        BinaryPrimitives.WriteInt32BigEndian(block, playerId);
        MARKER.CopyTo(block[4..]);

        using var aes = Aes.Create();
        aes.Key = key;

        return _prefix + Convert.ToHexStringLower(aes.EncryptEcb(block, PaddingMode.None));
    }

    /// <summary>The player an id names, when it is one this hotel gave out.</summary>
    public bool TryParse(string? uniqueId, out int playerId)
    {
        playerId = 0;

        if (
            uniqueId is null
            || uniqueId.Length != _prefix.Length + 32
            || !uniqueId.StartsWith(_prefix, StringComparison.Ordinal)
        )
            return false;

        Span<byte> sealedBlock = stackalloc byte[16];

        for (var i = 0; i < 16; i++)
        {
            if (
                !byte.TryParse(
                    uniqueId.AsSpan(_prefix.Length + (i * 2), 2),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out sealedBlock[i]
                )
            )
                return false;
        }

        using var aes = Aes.Create();
        aes.Key = key;

        var block = aes.DecryptEcb(sealedBlock, PaddingMode.None);

        if (!block.AsSpan(4).SequenceEqual(MARKER))
            return false;

        playerId = BinaryPrimitives.ReadInt32BigEndian(block);

        return playerId > 0;
    }
}
