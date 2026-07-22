// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           IPpeFactory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing Ppe aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Interfaces;

/// <summary>Factory interface for constructing <see cref="Ppe"/> aggregates.</summary>
public interface IPpeFactory
{
    /// <summary>Issues a new PPE item with the supplied parameters.</summary>
    PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe Create(
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
        int replacementCycleDays = 180);
}
