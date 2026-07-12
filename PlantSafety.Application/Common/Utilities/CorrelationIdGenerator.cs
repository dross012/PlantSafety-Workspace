// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: UUIDv7-based correlation and command identifier generator.
//   §1.1 (Deterministic Time Rule): all identifiers are time-ordered UUIDv7.
//   §1.2 (Deterministic Identifier Rule): raw Guids are forbidden in domain; UUIDv7 is required.
//   Generated once at the API boundary and passed inward; never regenerated for child operations.
// SchemaVersion: 1
// Additive-only evolution: new generator methods may be added; existing semantics must not change.

namespace PlantSafety.Application.Common.Utilities;

/// <summary>
/// Provides time-ordered UUIDv7 identifier generation for commands, events, and correlation lineage.
/// UUIDv7 encodes a millisecond-precision UTC timestamp in the high bits, enabling natural sort order
/// and time-based partitioning in Azure SQL.
/// </summary>
public static class CorrelationIdGenerator
{
    /// <summary>
    /// Generates a new UUIDv7 identifier using the current UTC time.
    /// </summary>
    /// <returns>A new time-ordered <see cref="Guid"/>.</returns>
    public static Guid NewId() => NewId(DateTimeOffset.UtcNow);

    /// <summary>
    /// Generates a new UUIDv7 identifier anchored to the given UTC timestamp.
    /// Used when the timestamp is injected externally (e.g., in tests or saga compensation steps).
    /// Implemented manually (rather than via <c>Guid.CreateVersion7</c>) because this project
    /// targets net8.0, and that API was introduced in .NET 9.
    /// </summary>
    /// <param name="timestamp">The UTC timestamp to embed in the UUIDv7.</param>
    /// <returns>A new time-ordered <see cref="Guid"/> anchored to the specified timestamp.</returns>
    public static Guid NewId(DateTimeOffset timestamp)
    {
        long unixMs = timestamp.ToUnixTimeMilliseconds();
        uint high = (uint)(unixMs >> 16);
        ushort mid = (ushort)(unixMs & 0xFFFF);

        var bytes = new byte[16];
        bytes[0] = (byte)high;
        bytes[1] = (byte)(high >> 8);
        bytes[2] = (byte)(high >> 16);
        bytes[3] = (byte)(high >> 24);
        bytes[4] = (byte)mid;
        bytes[5] = (byte)(mid >> 8);

        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes.AsSpan(6, 10));

        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x70); // Version 7 marker.
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // RFC 4122 variant marker.

        return new Guid(bytes);
    }

    /// <summary>
    /// Returns the embedded UTC timestamp from a UUIDv7 identifier.
    /// Returns <c>null</c> for non-UUIDv7 Guids.
    /// </summary>
    /// <param name="id">The UUIDv7 identifier to extract the timestamp from.</param>
    /// <returns>The embedded UTC timestamp, or <c>null</c> if the Guid is not UUIDv7.</returns>
    public static DateTimeOffset? ExtractTimestamp(Guid id)
    {
        var bytes = id.ToByteArray();
        var version = (bytes[7] >> 4) & 0xF;
        if (version != 7)
        {
            return null;
        }

        var high = (long)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
        var mid = (long)(bytes[4] | (bytes[5] << 8));
        var ms = (high << 16) | mid;
        return DateTimeOffset.FromUnixTimeMilliseconds(ms);
    }
}
