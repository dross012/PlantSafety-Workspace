// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           IPackagingSpecificationFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PackagingSpecification aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces;

/// <summary>Factory interface for constructing <c>PackagingSpecification</c> aggregates.</summary>
public interface IPackagingSpecificationFactory
{
    /// <summary>Authorizes a new PackagingSpecification with the supplied parameters.</summary>
    PackagingSpecification Create(
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
        Guid? siteId = null);
}
