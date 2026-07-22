// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for publishing a CultureAssessment report over HTTP.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetyculture/assessments/{cultureAssessmentId}/publish</c>.</summary>
/// <param name="Summary">Narrative summary included in the published report.</param>
public sealed record PublishCultureAssessmentReportRequest(string Summary);
