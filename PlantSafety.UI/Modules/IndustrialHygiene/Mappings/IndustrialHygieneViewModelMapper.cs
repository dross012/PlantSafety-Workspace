// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the
//   Industrial Hygiene module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

using PlantSafety.UI.Modules.IndustrialHygiene.Models;

namespace PlantSafety.UI.Modules.IndustrialHygiene.Mappings;

/// <summary>Presentation view model for an Industrial Hygiene row/detail, derived from <see cref="IndustrialHygieneItemDto"/>.</summary>
public sealed record IndustrialHygieneViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required IndustrialHygieneItemDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="IndustrialHygieneItemDto.ExposureLevel"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="IndustrialHygieneItemDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Industrial Hygiene module.</summary>
public static class IndustrialHygieneViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps an <see cref="IndustrialHygieneItemDto"/> to its presentation <see cref="IndustrialHygieneViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static IndustrialHygieneViewModel ToViewModel(this IndustrialHygieneItemDto dto) => dto.Adapt<IndustrialHygieneViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<IndustrialHygieneItemDto, IndustrialHygieneViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.ExposureLevel))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToSeverityForBadge(string exposureLevel) => exposureLevel.Trim().ToLowerInvariant() switch
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
