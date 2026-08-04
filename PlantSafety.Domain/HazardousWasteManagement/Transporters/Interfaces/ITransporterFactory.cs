// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           ITransporterFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing Transporter aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Interfaces;

/// <summary>Factory interface for constructing Transporter aggregates.</summary>
public interface ITransporterFactory
{
    /// <summary>Registers a new Transporter aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string companyName, string epaTransporterId, UtcTimestamp? licenseExpiresAtUtc);
}
