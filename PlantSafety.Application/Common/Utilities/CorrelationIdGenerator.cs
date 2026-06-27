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
    public static Guid NewId() => Guid.CreateVersion7();

    /// <summary>
    /// Generates a new UUIDv7 identifier anchored to the given UTC timestamp.
    /// Used when the timestamp is injected externally (e.g., in tests or saga compensation steps).
    /// </summary>
    /// <param name="timestamp">The UTC timestamp to embed in the UUIDv7.</param>
    /// <returns>A new time-ordered <see cref="Guid"/> anchored to the specified timestamp.</returns>
    public static Guid NewId(DateTimeOffset timestamp)
    {
        return Guid.CreateVersion7(timestamp);
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
