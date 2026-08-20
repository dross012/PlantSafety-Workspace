// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-20 — converted from Mapster (reflection-based mapping)
//   to plain, explicit, deterministic C# mapping per Enterprise Handbook
//   Prohibited Technology Rule 2.16 (reflection-based mapping is
//   prohibited); this closes out a deferred follow-up from an earlier
//   remediation pass.
// FILE DESCRIPTION: Explicit DTO -> ViewModel mapping for the Crisis
//   Management module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.CrisisManagement.Models;

namespace PlantSafety.UI.Modules.CrisisManagement.Mappings;

/// <summary>Presentation view model for a Crisis Event row/detail, derived from <see cref="CrisisEventDto"/>.</summary>
public sealed record CrisisEventViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required CrisisEventDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="CrisisEventDto.Status"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="CrisisEventDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Explicit deterministic DTO -&gt; ViewModel mapping for the Crisis Management module.</summary>
public static class CrisisEventViewModelMapper
{
    /// <summary>Maps a <see cref="CrisisEventDto"/> to its presentation <see cref="CrisisEventViewModel"/>.</summary>
    /// <param name="dto">The source DTO.</param>
    public static CrisisEventViewModel ToViewModel(this CrisisEventDto dto) => new()
    {
        Source = dto,
        SeverityForBadge = ToSeverityForBadge(dto.Status),
        OperationalStatus = ToOperationalStatus(dto.Status),
    };

    private static string ToSeverityForBadge(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" => "Critical",
        "contained" => "High",
        "resolved" => "Medium",
        "closedout" => "Low",
        _ => "Info",
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" => "Critical",
        "contained" => "Warning",
        "resolved" or "closedout" => "Healthy",
        _ => "Offline",
    };
}
