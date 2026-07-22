// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsFactory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing LotoPrograms aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Factories;

/// <summary>Default factory implementation for constructing <see cref="LotoPrograms"/> aggregates.</summary>
public sealed class LotoProgramsFactory : ILotoProgramsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms Document(
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
        UserId authorizedByUserId)
        => PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms.Document(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, programTitle, equipmentTag, energySourceTypes, isolationPointDescription, isolationPointCount, authorizedByUserId);
}
