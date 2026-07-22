// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for creating a new CultureAssessment over HTTP.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetyculture/assessments</c>.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="AssessmentTitle">Human-readable assessment title.</param>
/// <param name="PeriodStartUtc">Inclusive start of the assessment period (UTC).</param>
/// <param name="PeriodEndUtc">Inclusive end of the assessment period (UTC).</param>
/// <param name="Summary">Narrative summary of the assessment scope.</param>
public sealed record CreateCultureAssessmentRequest(
    Guid? SiteId,
    string AssessmentTitle,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    string Summary);
