// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the Emergency
//   Management module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

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

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Emergency Management module.</summary>
public static class EmergencyResponseViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps an <see cref="EmergencyResponseDto"/> to its presentation <see cref="EmergencyResponseViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static EmergencyResponseViewModel ToViewModel(this EmergencyResponseDto dto) => dto.Adapt<EmergencyResponseViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<EmergencyResponseDto, EmergencyResponseViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.Status))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

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
