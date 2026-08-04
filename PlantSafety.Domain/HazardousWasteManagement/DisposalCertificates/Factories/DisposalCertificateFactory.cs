// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing DisposalCertificate aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Factories;

/// <summary>Default factory implementation for constructing DisposalCertificate aggregates.</summary>
public sealed class DisposalCertificateFactory : IDisposalCertificateFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, Guid manifestId, Guid tsdfFacilityId,
        DisposalMethod disposalMethod, UtcTimestamp disposalDateUtc)
        => PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId,
            manifestId, tsdfFacilityId, disposalMethod, disposalDateUtc);
}
