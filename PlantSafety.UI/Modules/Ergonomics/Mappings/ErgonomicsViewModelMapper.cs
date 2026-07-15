// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the
//   Ergonomics module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

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

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Ergonomics module.</summary>
public static class ErgonomicsViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps an <see cref="ErgonomicsItemDto"/> to its presentation <see cref="ErgonomicsViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static ErgonomicsViewModel ToViewModel(this ErgonomicsItemDto dto) => dto.Adapt<ErgonomicsViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<ErgonomicsItemDto, ErgonomicsViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.RiskLevel))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

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
