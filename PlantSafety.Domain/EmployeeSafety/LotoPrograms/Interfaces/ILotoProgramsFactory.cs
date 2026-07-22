// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           ILotoProgramsFactory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing LotoPrograms aggregates via Document().
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Interfaces;

/// <summary>Factory interface for constructing <see cref="LotoPrograms"/> aggregates.</summary>
public interface ILotoProgramsFactory
{
    /// <summary>Documents a new LOTO program with the supplied parameters.</summary>
    PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms Document(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string programTitle,
        string equipmentTag,
        LotoEnergySourceType energySourceTypes,
        string isolationPointDescription,
        int isolationPointCount,
        UserId authorizedByUserId);
}
