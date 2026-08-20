// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-20 — converted from Mapster (reflection-based mapping)
//   to plain, explicit, deterministic C# mapping per Enterprise Handbook
//   Prohibited Technology Rule 2.16 (reflection-based mapping is
//   prohibited); this closes out a deferred follow-up from an earlier
//   remediation pass.
// FILE DESCRIPTION: Explicit DTO -> ViewModel mapping for the
//   Observation module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.Observations.Models;

namespace PlantSafety.UI.Modules.Observations.Mappings;

/// <summary>Presentation view model for an Observation row/detail, derived from <see cref="ObservationDto"/>.</summary>
public sealed record ObservationViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required ObservationDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="ObservationDto.ObservationType"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="ObservationDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Explicit deterministic DTO -&gt; ViewModel mapping for the Observation module.</summary>
public static class ObservationViewModelMapper
{
    /// <summary>Maps an <see cref="ObservationDto"/> to its presentation <see cref="ObservationViewModel"/>.</summary>
    /// <param name="dto">The source DTO.</param>
    public static ObservationViewModel ToViewModel(this ObservationDto dto) => new()
    {
        Source = dto,
        SeverityForBadge = ToSeverityForBadge(dto.ObservationType),
        OperationalStatus = ToOperationalStatus(dto.Status),
    };

    private static string ToSeverityForBadge(string observationType) => observationType.Trim().ToLowerInvariant() switch
    {
        "unsafe" => "High",
        "atrisk" => "Medium",
        "positive" => "Low",
        _ => "Info",
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "closed" => "Healthy",
        "actionrequired" => "Critical",
        "open" => "Warning",
        _ => "Offline",
    };
}
