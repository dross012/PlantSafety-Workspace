// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the RCA
//   module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

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

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the RCA module.</summary>
public static class RcaViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps a <see cref="RootCauseAnalysisDto"/> to its presentation <see cref="RcaViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static RcaViewModel ToViewModel(this RootCauseAnalysisDto dto) => dto.Adapt<RcaViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<RootCauseAnalysisDto, RcaViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.IsCompleted, src => src.CompletedAtUtc.HasValue)
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "approved" or "closed" => "Healthy",
        "underreview" => "Warning",
        "draft" => "Offline",
        _ => "Offline",
    };
}
