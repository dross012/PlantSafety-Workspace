// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the
//   Observation module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

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

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Observation module.</summary>
public static class ObservationViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps an <see cref="ObservationDto"/> to its presentation <see cref="ObservationViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static ObservationViewModel ToViewModel(this ObservationDto dto) => dto.Adapt<ObservationViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<ObservationDto, ObservationViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.ObservationType))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

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
