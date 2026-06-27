// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Alerts
//  Layer:          Infrastructure
//  File:           IAlertsInfrastructure.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Infrastructure contract for the Alerts module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Alerts.Interfaces;

/// <summary>
/// Infrastructure service contract for the <c>Alerts</c> module.
/// Defines persistence and messaging operations beyond the domain repository interface.
/// </summary>
public interface IAlertsInfrastructure
{
    /// <summary>Persists a new Alerts record via the insert stored procedure.</summary>
    Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a Alerts record exists for the given identifier.</summary>
    Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
