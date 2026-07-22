// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for recording a CultureAssessment score over HTTP.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetyculture/assessments/{cultureAssessmentId}/score</c>.</summary>
/// <param name="OverallCultureScore">Overall computed culture score.</param>
/// <param name="LeadershipEngagementScore">Optional leadership engagement sub-score.</param>
public sealed record RecordCultureAssessmentScoreRequest(
    decimal OverallCultureScore,
    decimal? LeadershipEngagementScore);
