// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record/recompute the overall culture health score and leadership engagement score for a CultureAssessment period. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Records culture scores on a CultureAssessment under the SafetyCulture bounded context.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="CultureAssessmentId">Identity of the CultureAssessment to record scores on.</param>
/// <param name="OverallCultureScore">Overall culture health score (0-100).</param>
/// <param name="LeadershipEngagementScore">Optional leadership engagement/participation score (0-100).</param>
public sealed record RecordCultureAssessmentScoreCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid CultureAssessmentId,
    decimal OverallCultureScore,
    decimal? LeadershipEngagementScore) : ICommand<Result<RecordCultureAssessmentScoreResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<RecordCultureAssessmentScoreResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="RecordCultureAssessmentScoreCommand"/> from raw, untrusted transport values, generating
    /// envelope identifiers where the caller did not supply them.
    /// </summary>
    public static RecordCultureAssessmentScoreCommand FromDto(
        Guid ownerId,
        Guid cultureAssessmentId,
        decimal overallCultureScore,
        decimal? leadershipEngagementScore,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            OwnerId: ownerId,
            CultureAssessmentId: cultureAssessmentId,
            OverallCultureScore: overallCultureScore,
            LeadershipEngagementScore: leadershipEngagementScore);
}
