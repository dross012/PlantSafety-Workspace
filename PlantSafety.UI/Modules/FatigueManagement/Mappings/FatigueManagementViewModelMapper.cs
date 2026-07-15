// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the Fatigue
//   Management module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

using PlantSafety.UI.Modules.FatigueManagement.Models;

namespace PlantSafety.UI.Modules.FatigueManagement.Mappings;

/// <summary>Presentation view model for a Fatigue Management row/detail, derived from <see cref="FatigueManagementItemDto"/>.</summary>
public sealed record FatigueManagementViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required FatigueManagementItemDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="FatigueManagementItemDto.FatigueRiskLevel"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="FatigueManagementItemDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Fatigue Management module.</summary>
public static class FatigueManagementViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps a <see cref="FatigueManagementItemDto"/> to its presentation <see cref="FatigueManagementViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static FatigueManagementViewModel ToViewModel(this FatigueManagementItemDto dto) => dto.Adapt<FatigueManagementViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<FatigueManagementItemDto, FatigueManagementViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.FatigueRiskLevel))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToSeverityForBadge(string fatigueRiskLevel) => fatigueRiskLevel.Trim().ToLowerInvariant() switch
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
