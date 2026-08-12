// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           GenerateShippingPaperCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to generate and attach a DOT shipping paper to a HazmatShipment
//                  (Draft/Validated -> Validated). The handler loads the shipment, resolves its
//                  materials via the Classification module's Application-layer read repository,
//                  calls ShippingPaperGenerator.BuildShippingPaper(...), then calls
//                  aggregate.AttachShippingPaper(...). Implements IConcurrencyAwareCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Generates and attaches a DOT shipping paper to a HazmatShipment (Draft/Validated -&gt; Validated).</summary>
/// <param name="WasteManifestReference">Optional hazardous-waste manifest reference, when applicable.</param>
/// <param name="ShipperCertificationText">Optional shipper certification text override.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="HazmatShipmentId">Identifier of the HazmatShipment record.</param>
/// <param name="PerformedByUserId">Identifier of the user performing this operation.</param>
public sealed record GenerateShippingPaperCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazmatShipmentId,
    string? WasteManifestReference,
    string? ShipperCertificationText,
    Guid PerformedByUserId) : ICommand<Result<HazmatShipmentStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<HazmatShipmentStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
