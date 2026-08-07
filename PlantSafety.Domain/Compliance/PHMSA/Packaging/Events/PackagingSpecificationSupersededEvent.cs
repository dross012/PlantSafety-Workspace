// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationSupersededEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when an authorized PackagingSpecification is superseded by a newer
//                  authorization.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Events;

/// <summary>Raised when an authorized <c>PackagingSpecification</c> is superseded by a newer authorization.</summary>
public sealed record PackagingSpecificationSupersededEvent(
    PackagingSpecificationId PackagingSpecificationId,
    PackagingSpecificationId SupersededBySpecificationId,
    UtcTimestamp     SupersededAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IPackagingSpecificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PackagingSpecificationSuperseded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SupersededAtUtc;
}
