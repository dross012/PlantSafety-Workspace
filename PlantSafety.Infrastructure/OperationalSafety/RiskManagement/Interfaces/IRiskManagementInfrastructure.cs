// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.RiskManagement
//  Layer:          Infrastructure
//  File:           IRiskManagementInfrastructure.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Infrastructure contract for the RiskManagement module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Interfaces;

/// <summary>
/// Infrastructure service contract for the <c>RiskManagement</c> module.
/// Defines persistence and messaging operations beyond the domain repository interface.
/// </summary>
public interface IRiskManagementInfrastructure
{
    /// <summary>Persists a new RiskManagement record via the insert stored procedure.</summary>
    Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a RiskManagement record exists for the given identifier.</summary>
    Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
