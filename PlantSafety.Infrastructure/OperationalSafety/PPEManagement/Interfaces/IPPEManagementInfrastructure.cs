// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.PPEManagement
//  Layer:          Infrastructure
//  File:           IPPEManagementInfrastructure.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Infrastructure contract for the PPEManagement module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Interfaces;

/// <summary>
/// Infrastructure service contract for the <c>PPEManagement</c> module.
/// Defines persistence and messaging operations beyond the domain repository interface.
/// </summary>
public interface IPPEManagementInfrastructure
{
    /// <summary>Persists a new PPEManagement record via the insert stored procedure.</summary>
    Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a PPEManagement record exists for the given identifier.</summary>
    Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
