// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a CommunityImpactAssessment for an on-site incident ("CommunitySafety.CommunityImpactAssessment").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Records a community impact assessment for a specific on-site SafetyEvents incident.</summary>
public sealed record RecordCommunityImpactAssessmentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid SiteId,
    Guid IncidentId,
    string Severity,
    string AffectedArea,
    string Description,
    int? EstimatedPersonsAffected,
    Guid AssessedByUserId,
    Guid PerformedByUserId) : ICommand<Result<CommunityImpactAssessmentActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<CommunityImpactAssessmentActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
