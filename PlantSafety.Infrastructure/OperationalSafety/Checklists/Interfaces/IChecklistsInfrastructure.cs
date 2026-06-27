// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Checklists
//  Layer:          Infrastructure
//  File:           IChecklistsInfrastructure.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Infrastructure contract for the Checklists module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Checklists.Interfaces;

/// <summary>
/// Infrastructure service contract for the <c>Checklists</c> module.
/// Defines persistence and messaging operations beyond the domain repository interface.
/// </summary>
public interface IChecklistsInfrastructure
{
    /// <summary>Persists a new Checklists record via the insert stored procedure.</summary>
    Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a Checklists record exists for the given identifier.</summary>
    Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
