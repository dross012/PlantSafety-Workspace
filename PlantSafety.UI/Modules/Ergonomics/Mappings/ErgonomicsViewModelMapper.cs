// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-20 — converted from Mapster (reflection-based mapping)
//   to plain, explicit, deterministic C# mapping per Enterprise Handbook
//   Prohibited Technology Rule 2.16 (reflection-based mapping is
//   prohibited); this closes out a deferred follow-up from an earlier
//   remediation pass.
// FILE DESCRIPTION: Explicit DTO -> ViewModel mapping for the
//   Ergonomics module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.Ergonomics.Models;

namespace PlantSafety.UI.Modules.Ergonomics.Mappings;

/// <summary>Presentation view model for an Ergonomics row/detail, derived from <see cref="ErgonomicsItemDto"/>.</summary>
public sealed record ErgonomicsViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required ErgonomicsItemDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="ErgonomicsItemDto.RiskLevel"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="ErgonomicsItemDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Explicit deterministic DTO -&gt; ViewModel mapping for the Ergonomics module.</summary>
public static class ErgonomicsViewModelMapper
{
    /// <summary>Maps an <see cref="ErgonomicsItemDto"/> to its presentation <see cref="ErgonomicsViewModel"/>.</summary>
    /// <param name="dto">The source DTO.</param>
    public static ErgonomicsViewModel ToViewModel(this ErgonomicsItemDto dto) => new()
    {
        Source = dto,
        SeverityForBadge = ToSeverityForBadge(dto.RiskLevel),
        OperationalStatus = ToOperationalStatus(dto.Status),
    };

    private static string ToSeverityForBadge(string riskLevel) => riskLevel.Trim().ToLowerInvariant() switch
    {
        "high" => "High",
        "medium" => "Medium",
        "low" => "Low",
        _ => "Info",
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "closed" or "resolved" => "Healthy",
        "actionrequired" or "escalated" => "Critical",
        "open" => "Warning",
        _ => "Offline",
    };
}
