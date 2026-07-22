// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Application-layer repository abstraction for Widget aggregate persistence
//   and read-model queries. Implemented in the Infrastructure layer (Dapper/stored-procedure-only;
//   not included in this Domain+Application-only generation pass).
// SchemaVersion: 1

using PlantSafety.Application.Dashboards.Widget.Models;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Application.Dashboards.Widget.Repositories;

/// <summary>Application-layer persistence and read-model gateway for the Widget aggregate.</summary>
public interface IWidgetRepository
{
    /// <summary>Retrieves a Widget aggregate by identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Dashboards.Widget.Aggregates.Widget?> GetByIdAsync(WidgetId widgetId, CancellationToken cancellationToken);

    /// <summary>Persists a newly created Widget aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Dashboards.Widget.Aggregates.Widget aggregate, CancellationToken cancellationToken);

    /// <summary>Persists changes to an existing Widget aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Dashboards.Widget.Aggregates.Widget aggregate, CancellationToken cancellationToken);

    /// <summary>Retrieves a Widget read-model DTO by identifier, or <see langword="null"/> if not found.</summary>
    Task<WidgetDto?> GetDtoByIdAsync(Guid tenantId, Guid widgetId, CancellationToken cancellationToken);

    /// <summary>Returns the full reusable widget library (all Active widgets) for the tenant.</summary>
    Task<IReadOnlyList<WidgetDto>> GetWidgetLibraryAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Resolves the normalized data contract (Series/Categories/Metrics) for a single widget instance.</summary>
    Task<WidgetDataContractDto> GetWidgetDataAsync(Guid tenantId, Guid widgetId, IReadOnlyDictionary<string, string> filterOverrides, CancellationToken cancellationToken);
}
