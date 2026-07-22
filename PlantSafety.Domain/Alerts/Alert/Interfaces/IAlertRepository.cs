// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           IAlertRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Alert aggregate root. Implemented in the
//                  Infrastructure layer (Dapper/stored-procedure-only; not included in this
//                  Domain+Application-only generation pass).
//                  Additive-only evolution.
// ============================================================================
using AlertSpecification = PlantSafety.Domain.Alerts.Alert.Specifications.AlertSpecification;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Domain.Alerts.Alert.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.Alert"/> aggregate root.</summary>
public interface IAlertRepository
{
    /// <summary>Retrieves an Alert aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.Alert?> GetByIdAsync(AlertId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Alert aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.Alert>> ListAsync(AlertSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Alert aggregate.</summary>
    Task AddAsync(Aggregates.Alert aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Alert aggregate.</summary>
    Task UpdateAsync(Aggregates.Alert aggregate, CancellationToken cancellationToken = default);
}
