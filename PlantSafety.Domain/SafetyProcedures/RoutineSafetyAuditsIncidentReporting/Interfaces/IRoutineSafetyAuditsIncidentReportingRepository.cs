// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           IRoutineSafetyAuditsIncidentReportingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RoutineSafetyAuditsIncidentReporting aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Interfaces;

/// <summary>Domain repository interface for the <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate root.</summary>
public interface IRoutineSafetyAuditsIncidentReportingRepository
{
    /// <summary>Retrieves a RoutineSafetyAuditsIncidentReporting aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting?> GetByIdAsync(RoutineSafetyAuditsIncidentReportingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all RoutineSafetyAuditsIncidentReporting aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting>> ListAsync(RoutineSafetyAuditsIncidentReportingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RoutineSafetyAuditsIncidentReporting aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing RoutineSafetyAuditsIncidentReporting aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting aggregate, CancellationToken cancellationToken = default);
}