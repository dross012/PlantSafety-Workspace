// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           CreateRCACommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new ContinuousImprovement RCA record. Carries the full
//                  command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands
//                  .CreateRootCauseAnalysisCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Creates a new ContinuousImprovement RCA record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ProblemStatement">Problem statement being analyzed.</param>
/// <param name="Methodology">Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</param>
/// <param name="OwnerUserId">Identity of the user who owns the RCA.</param>
public sealed record CreateRCACommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ProblemStatement,
    string Methodology,
    Guid OwnerUserId) : ICommand<Result<RCAStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => OwnerUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RCAStatusResult>>.CausationId => CausationId;
}
