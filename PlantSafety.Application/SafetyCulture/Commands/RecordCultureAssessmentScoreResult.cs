// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for RecordCultureAssessmentScoreCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Result of a successful <see cref="RecordCultureAssessmentScoreCommand"/> execution.</summary>
/// <param name="CultureAssessmentId">Identity of the CultureAssessment the score was recorded on.</param>
/// <param name="Status">Lifecycle status after recording the score.</param>
/// <param name="OverallCultureScore">Recorded overall culture health score.</param>
public sealed record RecordCultureAssessmentScoreResult(
    Guid CultureAssessmentId,
    string Status,
    decimal OverallCultureScore);
