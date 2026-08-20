// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-20 — converted from Mapster (reflection-based mapping)
//   to plain, explicit, deterministic C# mapping per Enterprise Handbook
//   Prohibited Technology Rule 2.16 (reflection-based mapping is
//   prohibited); this closes out a deferred follow-up from an earlier
//   remediation pass. Adds presentation-only computed fields (badge
//   severity, days-open) on top of the wire DTO without mutating it.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using PlantSafety.UI.Modules.NearMiss.Models;

namespace PlantSafety.UI.Modules.NearMiss.Mappings;

/// <summary>Presentation view model for a Near Miss row/detail, derived from <see cref="NearMissDto"/>.</summary>
public sealed record NearMissViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required NearMissDto Source { get; init; }

    /// <summary>Gets the number of whole days between occurrence and now.</summary>
    public int DaysSinceOccurred { get; init; }

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="NearMissDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Explicit deterministic DTO -&gt; ViewModel mapping for the Near Miss module.</summary>
public static class NearMissViewModelMapper
{
    /// <summary>Maps a <see cref="NearMissDto"/> to its presentation <see cref="NearMissViewModel"/>.</summary>
    /// <param name="dto">The source DTO.</param>
    public static NearMissViewModel ToViewModel(this NearMissDto dto) => new()
    {
        Source = dto,
        DaysSinceOccurred = Math.Max(0, (int)(DateTimeOffset.UtcNow - dto.OccurredAtUtc).TotalDays),
        OperationalStatus = ToOperationalStatus(dto.Status),
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "closed" => "Healthy",
        "open" => "Warning",
        "actionrequired" or "escalated" => "Critical",
        _ => "Offline",
    };
}
