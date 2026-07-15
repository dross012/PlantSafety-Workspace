// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the Worker
//   WellBeing module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

using PlantSafety.UI.Modules.WorkerWellBeing.Models;

namespace PlantSafety.UI.Modules.WorkerWellBeing.Mappings;

/// <summary>Presentation view model for a Worker WellBeing row/detail, derived from <see cref="WorkerWellBeingDto"/>.</summary>
public sealed record WorkerWellBeingViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required WorkerWellBeingDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="WorkerWellBeingDto.WellBeingRiskLevel"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="WorkerWellBeingDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Worker WellBeing module.</summary>
public static class WorkerWellBeingViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps a <see cref="WorkerWellBeingDto"/> to its presentation <see cref="WorkerWellBeingViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static WorkerWellBeingViewModel ToViewModel(this WorkerWellBeingDto dto) => dto.Adapt<WorkerWellBeingViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<WorkerWellBeingDto, WorkerWellBeingViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.WellBeingRiskLevel))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToSeverityForBadge(string wellBeingRiskLevel) => wellBeingRiskLevel.Trim().ToLowerInvariant() switch
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
