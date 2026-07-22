// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Application-layer repository abstraction for Alert aggregate persistence,
//   used exclusively by Alert command handlers. Implemented in the Infrastructure layer
//   (Dapper/stored-procedure-only; not included in this Domain+Application-only generation pass).
// SchemaVersion: 1

using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Application-layer persistence gateway for the Alert aggregate, used by command handlers.</summary>
public interface IAlertCommandRepository
{
    /// <summary>Retrieves an Alert aggregate by identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Alerts.Alert.Aggregates.Alert?> GetByIdAsync(AlertId alertId, CancellationToken cancellationToken);

    /// <summary>Persists a newly triggered Alert aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Alerts.Alert.Aggregates.Alert aggregate, CancellationToken cancellationToken);

    /// <summary>Persists changes to an existing Alert aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Alerts.Alert.Aggregates.Alert aggregate, CancellationToken cancellationToken);
}
