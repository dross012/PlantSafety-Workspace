// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ApplyPlacardsCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to compute and record the required placard set for a HazmatContainer
//                  (MarkedAndLabeled -> PlacardingCalculated). The handler first invokes
//                  PlantSafety.Domain.Compliance.PHMSA.Placarding.Services.PlacardingEngine to
//                  compute the placard list from the supplied load materials, then calls
//                  aggregate.ApplyPlacards(...) with the computed result. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>A single load material item used to compute placarding for a HazmatContainer.</summary>
/// <param name="HazardClass">DOT hazard class/division discriminator (e.g. "3", "1.1", "8").</param>
/// <param name="Quantity">Quantity of this material present in the load.</param>
/// <param name="QuantityUnit">Unit for <paramref name="Quantity"/> (e.g. "L", "kg", "gal").</param>
public sealed record PlacardingMaterialItem(string HazardClass, decimal Quantity, string QuantityUnit);

/// <summary>
/// Computes and records the required placard set for a HazmatContainer
/// (MarkedAndLabeled -&gt; PlacardingCalculated).
/// </summary>
/// <param name="Materials">The hazard classes present in the load, with quantity and unit.</param>
/// <param name="IsBulkPackaging">Whether the load is carried in bulk packaging.</param>
public sealed record ApplyPlacardsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazmatContainerId,
    IReadOnlyList<PlacardingMaterialItem> Materials,
    bool IsBulkPackaging,
    Guid PerformedByUserId) : ICommand<Result<HazmatContainerStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<HazmatContainerStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
