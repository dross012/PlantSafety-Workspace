// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-20 — converted from Mapster (reflection-based mapping)
//   to plain, explicit, deterministic C# mapping per Enterprise Handbook
//   Prohibited Technology Rule 2.16 (reflection-based mapping is
//   prohibited); this closes out a deferred follow-up from an earlier
//   remediation pass.
// FILE DESCRIPTION: Explicit DTO -> ViewModel mapping for the Emergency
//   Management module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.EmergencyManagement.Models;

namespace PlantSafety.UI.Modules.EmergencyManagement.Mappings;

/// <summary>Presentation view model for an Emergency Response row/detail, derived from <see cref="EmergencyResponseDto"/>.</summary>
public sealed record EmergencyResponseViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required EmergencyResponseDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="EmergencyResponseDto.Status"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="EmergencyResponseDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Explicit deterministic DTO -&gt; ViewModel mapping for the Emergency Management module.</summary>
public static class EmergencyResponseViewModelMapper
{
    /// <summary>Maps an <see cref="EmergencyResponseDto"/> to its presentation <see cref="EmergencyResponseViewModel"/>.</summary>
    /// <param name="dto">The source DTO.</param>
    public static EmergencyResponseViewModel ToViewModel(this EmergencyResponseDto dto) => new()
    {
        Source = dto,
        SeverityForBadge = ToSeverityForBadge(dto.Status),
        OperationalStatus = ToOperationalStatus(dto.Status),
    };

    private static string ToSeverityForBadge(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" => "Critical",
        "standdown" => "Medium",
        "completed" => "Low",
        _ => "Info",
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" => "Critical",
        "standdown" => "Warning",
        "completed" => "Healthy",
        _ => "Offline",
    };
}
