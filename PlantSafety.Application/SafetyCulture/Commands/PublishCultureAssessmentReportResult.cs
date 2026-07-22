// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for PublishCultureAssessmentReportCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Result of a successful <see cref="PublishCultureAssessmentReportCommand"/> execution.</summary>
/// <param name="CultureAssessmentId">Identity of the published CultureAssessment.</param>
/// <param name="Status">Lifecycle status after publication.</param>
/// <param name="OverallCultureScore">Published overall culture health score.</param>
public sealed record PublishCultureAssessmentReportResult(
    Guid CultureAssessmentId,
    string Status,
    decimal? OverallCultureScore);
