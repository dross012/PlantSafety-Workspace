// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           IRegulatoryCalendarRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RegulatoryCalendar aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;

/// <summary>Domain repository interface for the <see cref="RegulatoryCalendar"/> aggregate root.</summary>
public interface IRegulatoryCalendarRepository
{
    /// <summary>Retrieves a RegulatoryCalendar aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar?> GetByIdAsync(RegulatoryCalendarId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all RegulatoryCalendar aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar>> ListAsync(RegulatoryCalendarSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RegulatoryCalendar aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing RegulatoryCalendar aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate, CancellationToken cancellationToken = default);
}
