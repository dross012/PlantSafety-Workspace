// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           IDisposalCertificateFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing DisposalCertificate aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces;

/// <summary>Factory interface for constructing DisposalCertificate aggregates.</summary>
public interface IDisposalCertificateFactory
{
    /// <summary>Records a new DisposalCertificate aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, Guid manifestId, Guid tsdfFacilityId,
        DisposalMethod disposalMethod, UtcTimestamp disposalDateUtc);
}
