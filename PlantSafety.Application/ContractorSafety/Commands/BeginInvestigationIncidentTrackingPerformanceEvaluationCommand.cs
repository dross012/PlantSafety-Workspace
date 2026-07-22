// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to BeginInvestigation (lifecycle transition) for IncidentTrackingPerformanceEvaluation ("ContractorSafety.IncidentTrackingPerformanceEvaluation").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Invokes <c>IncidentTrackingPerformanceEvaluation.BeginInvestigation</c> on an existing IncidentTrackingPerformanceEvaluation aggregate.</summary>
public sealed record BeginInvestigationIncidentTrackingPerformanceEvaluationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid IncidentTrackingPerformanceEvaluationId,
    Guid EvaluatedByUserId,
    Guid PerformedByUserId) : ICommand<Result<IncidentTrackingPerformanceEvaluationActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<IncidentTrackingPerformanceEvaluationActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
