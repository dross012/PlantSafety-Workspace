// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           CreateRootCauseAnalysisCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to start a new RootCauseAnalysis engagement under the
//                  SafetyEvents.RootCauseAnalysis bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Commands
//                  .RaisePredictiveMaintenanceAlertCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;

/// <summary>Starts a new RootCauseAnalysis engagement.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="RelatedEventId">Identifier of the related incident, near-miss, or other safety event, if any.</param>
/// <param name="Methodology">Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</param>
/// <param name="LeadAnalystUserId">Identity of the lead analyst.</param>
public sealed record CreateRootCauseAnalysisCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid? RelatedEventId,
    string Methodology,
    Guid LeadAnalystUserId) : ICommand<Result<RootCauseAnalysisStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => LeadAnalystUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RootCauseAnalysisStatusResult>>.CausationId => CausationId;
}
