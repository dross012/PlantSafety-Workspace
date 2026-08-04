// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           ICyberPhysicalIncidentRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the CyberPhysicalIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Specifications;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.CyberPhysicalIncident"/> aggregate root.</summary>
public interface ICyberPhysicalIncidentRepository
{
    /// <summary>Retrieves a CyberPhysicalIncident by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.CyberPhysicalIncident?> GetByIdAsync(CyberPhysicalIncidentId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all CyberPhysicalIncidents matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.CyberPhysicalIncident>> ListAsync(Guid tenantId, CyberPhysicalIncidentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new CyberPhysicalIncident.</summary>
    Task AddAsync(Aggregates.CyberPhysicalIncident aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CyberPhysicalIncident.</summary>
    Task UpdateAsync(Aggregates.CyberPhysicalIncident aggregate, CancellationToken cancellationToken = default);
}
