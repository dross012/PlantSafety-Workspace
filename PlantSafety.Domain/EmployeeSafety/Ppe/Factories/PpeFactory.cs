// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeFactory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing Ppe aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Factories;

/// <summary>Default factory implementation for constructing <see cref="Ppe"/> aggregates.</summary>
public sealed class PpeFactory : IPpeFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string itemDescription,
        PpeCategory category,
        string assignedZone,
        UserId issuedToUserId,
        bool requiresFitTest,
        int replacementCycleDays = 180)
        => PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, itemDescription, category, assignedZone, issuedToUserId, requiresFitTest, replacementCycleDays);
}
