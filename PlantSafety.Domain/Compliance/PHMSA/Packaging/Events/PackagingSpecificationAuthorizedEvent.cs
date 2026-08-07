// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationAuthorizedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PackagingSpecification aggregate is created and authorized.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Events;

/// <summary>Raised when a new <c>PackagingSpecification</c> aggregate is created and authorized.</summary>
public sealed record PackagingSpecificationAuthorizedEvent(
    PackagingSpecificationId PackagingSpecificationId,
    DOTSpec          DOTSpec,
    UtcTimestamp     AuthorizedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IPackagingSpecificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PackagingSpecificationAuthorized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AuthorizedAtUtc;
}
