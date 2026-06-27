// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Schedules
//  Layer:          Infrastructure
//  File:           ISchedulesInfrastructure.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Infrastructure contract for the Schedules module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Schedules.Interfaces;

/// <summary>
/// Infrastructure service contract for the <c>Schedules</c> module.
/// Defines persistence and messaging operations beyond the domain repository interface.
/// </summary>
public interface ISchedulesInfrastructure
{
    /// <summary>Persists a new Schedules record via the insert stored procedure.</summary>
    Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a Schedules record exists for the given identifier.</summary>
    Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
