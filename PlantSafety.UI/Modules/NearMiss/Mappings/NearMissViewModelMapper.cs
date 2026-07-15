// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — converted from hand-written mapping to Mapster
//   TypeAdapterConfig per UI Platform Master Architecture Vol.0 §6 (Mapster
//   is the mandatory UI-layer DTO -> ViewModel mapping mechanism; the
//   Enterprise Handbook's AutoMapper/reflection prohibition governs
//   backend Domain/Application CQRS mapping paths only, not this layer).
//   Adds presentation-only computed fields (badge severity, days-open) on
//   top of the wire DTO without mutating it.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using Mapster;

using PlantSafety.UI.Modules.NearMiss.Models;

namespace PlantSafety.UI.Modules.NearMiss.Mappings;

/// <summary>Presentation view model for a Near Miss row/detail, derived from <see cref="NearMissDto"/>.</summary>
public sealed record NearMissViewModel
{
    /// <summary>Gets the source DTO.</summary>
    public required NearMissDto Source { get; init; }

    /// <summary>Gets the number of whole days between occurrence and now.</summary>
    public int DaysSinceOccurred { get; init; }

    /// <summary>Gets the StatusIndicatorComponent-compatible operational state derived from <see cref="NearMissDto.Status"/>.</summary>
    public string OperationalStatus { get; init; } = "Offline";
}

/// <summary>Mapster DTO -&gt; ViewModel projection config and call-site adapters for the Near Miss module.</summary>
public static class NearMissViewModelMapper
{
    private static readonly TypeAdapterConfig Config = CreateConfig();

    /// <summary>Maps a <see cref="NearMissDto"/> to its presentation <see cref="NearMissViewModel"/> via Mapster.</summary>
    /// <param name="dto">The source DTO.</param>
    public static NearMissViewModel ToViewModel(this NearMissDto dto) => dto.Adapt<NearMissViewModel>(Config);

    /// <summary>Builds the module's Mapster <see cref="TypeAdapterConfig"/>, registering the DTO -&gt; ViewModel projection.</summary>
    private static TypeAdapterConfig CreateConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<NearMissDto, NearMissViewModel>()
            .Map(dest => dest.Source, src => src)
            .Map(dest => dest.DaysSinceOccurred, src => Math.Max(0, (int)(DateTimeOffset.UtcNow - src.OccurredAtUtc).TotalDays))
            .Map(dest => dest.OperationalStatus, src => ToOperationalStatus(src.Status));
        return config;
    }

    private static string ToOperationalStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "closed" => "Healthy",
        "open" => "Warning",
        "actionrequired" or "escalated" => "Critical",
        _ => "Offline",
    };
}
