// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Mapping helpers projecting the Culture aggregate to Application-layer response DTOs.
// SchemaVersion: 1

using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Application.Analytics.Culture.Mappings;

/// <summary>Response DTO projecting a <see cref="Culture"/> aggregate.</summary>
/// <param name="CultureId">Underlying identifier value of the Culture aggregate.</param>
/// <param name="TenantId">Tenant that owns this culture record.</param>
/// <param name="SurveyPeriod">Survey period this measurement covers.</param>
/// <param name="CultureIndexScore">Computed safety culture index score (0-100).</param>
/// <param name="EngagementScore">Computed engagement score (0-100).</param>
/// <param name="ParticipationRate">Survey participation rate (0-100).</param>
/// <param name="TopRiskTheme">Top risk theme identified from this survey period.</param>
public sealed record CultureResponse(
    Guid CultureId,
    Guid TenantId,
    string SurveyPeriod,
    decimal CultureIndexScore,
    decimal EngagementScore,
    decimal ParticipationRate,
    string TopRiskTheme);

/// <summary>Static mapping helpers for the <see cref="Culture"/> aggregate.</summary>
public static class CultureMappings
{
    /// <summary>Projects a <see cref="Culture"/> aggregate into a <see cref="CultureResponse"/>.</summary>
    public static CultureResponse ToResponse(CultureAggregate culture) => new(
        CultureId:         culture.CultureId.Value,
        TenantId:          culture.TenantId,
        SurveyPeriod:      culture.SurveyPeriod,
        CultureIndexScore: culture.CultureIndexScore,
        EngagementScore:   culture.EngagementScore,
        ParticipationRate: culture.ParticipationRate,
        TopRiskTheme:      culture.TopRiskTheme);
}
