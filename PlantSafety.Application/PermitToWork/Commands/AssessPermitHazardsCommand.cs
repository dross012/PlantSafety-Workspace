// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record the safety officer's hazard assessment and required
// controls for a permit request (PTW Workflow Step 2 - Hazard Assessment).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Records the hazard assessment and required controls for a PermitWorkflow (PTW Workflow Step 2).</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PermitWorkflowId">Identity of the PermitWorkflow to assess.</param>
/// <param name="RequiredControls">Required controls identified (LOTO, PPE, ventilation, fire watch, barricades, atmospheric testing).</param>
/// <param name="RiskScore">Risk score assigned by the safety officer.</param>
/// <param name="PerformedByUserId">Identity of the safety officer performing the assessment.</param>
public sealed record AssessPermitHazardsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitWorkflowId,
    string RequiredControls,
    int RiskScore,
    Guid PerformedByUserId) : ICommand<Result<PermitWorkflowActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitWorkflowActionResult>>.CausationId => CausationId;
}
