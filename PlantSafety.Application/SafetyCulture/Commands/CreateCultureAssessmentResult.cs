// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateCultureAssessmentCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Result of a successful <see cref="CreateCultureAssessmentCommand"/> execution.</summary>
/// <param name="CultureAssessmentId">Identity of the newly created CultureAssessment aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateCultureAssessmentResult(
    Guid CultureAssessmentId,
    string Status);