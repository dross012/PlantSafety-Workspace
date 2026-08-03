// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           SurfaceRecommendationCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command attaching a predictive recommendation to a WorkflowInstance's current
//                  PredictiveRecommendation step. The recommendation is sourced (by the caller,
//                  typically an integration-event handler) from a predictive-analytics module such
//                  as Intelligence.PredictiveSafety or Analytics.PredictiveAnalytics -- consumed
//                  here purely as an opaque string reference, never a direct Domain-to-Domain
//                  reference (Handbook Part IV §4.2.2 Zero Cross-Module Leakage). Phase 5.3 human +
//                  machine collaboration: "Predictive recommendation steps".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Attaches a predictive recommendation to a WorkflowInstance's current step.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the instance whose current step the recommendation applies to.</param>
/// <param name="RecommendationReference">
/// Opaque reference to the source recommendation (e.g. an Intelligence.PredictiveSafety alert id
/// or Analytics.PredictiveAnalytics forecast id), carried as a string to avoid a compile-time
/// dependency on either module's Domain layer.
/// </param>
/// <param name="RequestedByUserId">Identity of the system process (or user) surfacing the recommendation.</param>
public sealed record SurfaceRecommendationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowInstanceId,
    string RecommendationReference,
    Guid RequestedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>, IWorkflowInstanceScopedCommand
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;
}
