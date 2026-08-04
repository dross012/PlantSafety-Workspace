// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing Transporter aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Factories;

/// <summary>Default factory implementation for constructing Transporter aggregates.</summary>
public sealed class TransporterFactory : ITransporterFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string companyName, string epaTransporterId, UtcTimestamp? licenseExpiresAtUtc)
        => PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, companyName, epaTransporterId, licenseExpiresAtUtc);
}
