// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationRetiredEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a PackagingSpecification record is retired.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Events;

/// <summary>Raised when a <c>PackagingSpecification</c> record is retired.</summary>
public sealed record PackagingSpecificationRetiredEvent(
    PackagingSpecificationId PackagingSpecificationId,
    UtcTimestamp     RetiredAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IPackagingSpecificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PackagingSpecificationRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}
