// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           CreateProductionLineCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to register a new ProductionLine master-data record -- the genuinely
//                  new, minimal master-data concept this module introduces (see
//                  docs/Phase5/ui-gaps-scoping.md "ProductionTracking" section). Carries the full
//                  command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Operational.WorkOrders.Commands.CreateWorkOrderCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Registers a new ProductionLine master-data record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="LineCode">Human-readable line code (format LINE-###).</param>
/// <param name="LineName">Line display name.</param>
/// <param name="RequestedByUserId">Identity of the user registering the line.</param>
public sealed record CreateProductionLineCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string LineCode,
    string LineName,
    Guid RequestedByUserId) : ICommand<Result<CreateProductionLineResult>>
{
    /// <summary>Identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateProductionLineResult>>.CausationId => CausationId;
}

/// <summary>Result of a successful <see cref="CreateProductionLineCommand"/> execution.</summary>
/// <param name="ProductionLineId">Identity of the newly registered ProductionLine aggregate.</param>
/// <param name="LineCode">Human-readable line code.</param>
public sealed record CreateProductionLineResult(Guid ProductionLineId, string LineCode);
