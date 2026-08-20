// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-20 — converted from Mapster (reflection-based mapping)
//   to plain, explicit, deterministic C# mapping per Enterprise Handbook
//   Prohibited Technology Rule 2.16 (reflection-based mapping is
//   prohibited); this closes out a deferred follow-up from an earlier
//   remediation pass.
// FILE DESCRIPTION: Explicit DTO -> ViewModel mapping for the RCA
//   module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.Rca.Models;

namespace PlantSafety.UI.Modules.Rca.Mappings;

/// <summary>Presentation view model for an RCA row/detail, derived from <see cref="RootCauseAnalysisDto"/>.</summary>
public sealed record RcaViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required RootCauseAnalysisDto Source { get; init; }

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="RootCauseAnalysisDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";

    /// <summary>Gets a value indicating whether this RCA has completed its investigation.</summary>
    public bool IsCompleted { get; init; }
}

/// <summary>Explicit deterministic DTO -&gt; ViewModel mapping for the RCA module.</summary>
public static class RcaViewModelMapper
{
    /// <summary>Maps a <see cref="RootCauseAnalysisDto"/> to its presentation <see cref="RcaViewModel"/>.</summary>
    /// <param name="dto">The source DTO.</param>
    public static RcaViewModel ToViewModel(this RootCauseAnalysisDto dto) => new()
    {
        Source = dto,
        IsCompleted = dto.CompletedAtUtc.HasValue,
        OperationalStatus = ToOperationalStatus(dto.Status),
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "approved" or "closed" => "Healthy",
        "underreview" => "Warning",
        "draft" => "Offline",
        _ => "Offline",
    };
}
