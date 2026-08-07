// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           CreateHazmatShipmentCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to create a new HazmatShipment record under the PHMSA ShippingPapers
//                  bounded context. Carries the full command envelope (CorrelationId, CausationId,
//                  CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Creates a new HazmatShipment record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="MaterialIds">Classified materials contained in this shipment (Classification module). Requires at least one.</param>
/// <param name="PackagingIds">Authorized packaging specifications used for this shipment (Packaging module).</param>
/// <param name="Mode">Mode of transport (Ground, Air, Vessel, Rail).</param>
/// <param name="RequestedByUserId">Identity of the user requesting creation.</param>
public sealed record CreateHazmatShipmentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    IReadOnlyList<Guid> MaterialIds,
    IReadOnlyList<Guid> PackagingIds,
    string Mode,
    Guid RequestedByUserId) : ICommand<Result<CreateHazmatShipmentResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateHazmatShipmentResult>>.CausationId => CausationId;
}

/// <summary>Result of a successful <see cref="CreateHazmatShipmentCommand"/> execution.</summary>
/// <param name="HazmatShipmentId">Identity of the newly created HazmatShipment aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateHazmatShipmentResult(Guid HazmatShipmentId, string Status);
