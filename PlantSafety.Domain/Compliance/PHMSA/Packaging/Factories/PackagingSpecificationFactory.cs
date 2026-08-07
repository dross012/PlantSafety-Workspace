// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PackagingSpecification aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Factories;

/// <summary>Default factory implementation for constructing <c>PackagingSpecification</c> aggregates.</summary>
public sealed class PackagingSpecificationFactory : IPackagingSpecificationFactory
{
    /// <inheritdoc/>
    public PackagingSpecification Create(
        DOTSpec dotSpec,
        IReadOnlyList<string> performanceTests,
        decimal maxCapacity,
        string capacityUnit,
        string closureInstructions,
        string? reuseRestrictions,
        HazmatMaterialId? associatedMaterialId,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId = null)
        => PackagingSpecification.Create(
            dotSpec, performanceTests, maxCapacity, capacityUnit, closureInstructions, reuseRestrictions,
            associatedMaterialId, createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId);
}
