// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the Crisis
//   Management module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

using PlantSafety.UI.Modules.CrisisManagement.Models;

namespace PlantSafety.UI.Modules.CrisisManagement.Mappings;

/// <summary>Presentation view model for a Crisis Event row/detail, derived from <see cref="CrisisEventDto"/>.</summary>
public sealed record CrisisEventViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required CrisisEventDto Source { get; init; }

    /// <summary>Gets the SeverityBadgeComponent-compatible severity derived from <see cref="CrisisEventDto.Status"/>.</summary>
    public string SeverityForBadge { get; init; } = "Info";

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="CrisisEventDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Crisis Management module.</summary>
public static class CrisisEventViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps a <see cref="CrisisEventDto"/> to its presentation <see cref="CrisisEventViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static CrisisEventViewModel ToViewModel(this CrisisEventDto dto) => dto.Adapt<CrisisEventViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<CrisisEventDto, CrisisEventViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.SeverityForBadge, src => ToSeverityForBadge(src.Status))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToSeverityForBadge(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" => "Critical",
        "contained" => "High",
        "resolved" => "Medium",
        "closedout" => "Low",
        _ => "Info",
    };

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" => "Critical",
        "contained" => "Warning",
        "resolved" or "closedout" => "Healthy",
        _ => "Offline",
    };
}
