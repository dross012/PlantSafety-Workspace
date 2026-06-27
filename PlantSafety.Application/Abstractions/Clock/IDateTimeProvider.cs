// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Deterministic UTC clock abstraction for all timestamp generation.
//   All timestamps MUST use UTC only (§1.1 Deterministic Time Rule).
//   This interface is the ONLY permitted source of current time in Application and Domain layers.
//   DateTime.Now, DateTime.UtcNow, and DateTimeOffset.Now are prohibited in all non-infrastructure code.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or changed.

namespace PlantSafety.Application.Abstractions.Clock;

/// <summary>
/// Deterministic UTC clock abstraction. All Application and Domain layer code MUST obtain
/// timestamps from this interface — never from <c>DateTime.Now</c>, <c>DateTime.UtcNow</c>,
/// or any system locale-dependent source.
/// </summary>
/// <remarks>
/// Fulfills the Deterministic Time Rule (§1.1): all timestamps are UTC, generated once at
/// the boundary and passed inward. Infrastructure provides the real implementation;
/// tests inject a deterministic fixed-time stub.
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>Returns the current UTC date and time with millisecond precision.</summary>
    /// <returns>A <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> set to the current instant.</returns>
    DateTime UtcNow { get; }

    /// <summary>Returns the current UTC date and time as a <see cref="DateTimeOffset"/>.</summary>
    DateTimeOffset UtcNowOffset { get; }

    /// <summary>Returns today's date in UTC.</summary>
    DateOnly UtcToday { get; }
}
