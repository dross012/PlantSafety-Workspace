// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
// FILE DESCRIPTION: Mapster-based DTO -> ViewModel mapping for the Drills &
//   Exercises module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using Mapster;

using PlantSafety.UI.Modules.Drills.Models;

namespace PlantSafety.UI.Modules.Drills.Mappings;

/// <summary>Presentation view model for a Drill row/detail, derived from <see cref="DrillDto"/>.</summary>
public sealed record DrillViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required DrillDto Source { get; init; }

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="DrillDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";

    /// <summary>Gets a value indicating whether the drill has already been executed.</summary>
    public bool WasExecuted { get; init; }
}

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Drills &amp; Exercises module.</summary>
public static class DrillViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps a <see cref="DrillDto"/> to its presentation <see cref="DrillViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static DrillViewModel ToViewModel(this DrillDto dto) => dto.Adapt<DrillViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<DrillDto, DrillViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.WasExecuted, src => src.ExecutedAtUtc.HasValue)
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "completed" => "Healthy",
        "scheduled" => "Warning",
        "cancelled" => "Offline",
        _ => "Offline",
    };
}
