// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           IPpeRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Ppe aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Interfaces;

/// <summary>Domain repository interface for the <see cref="Ppe"/> aggregate root.</summary>
public interface IPpeRepository
{
    /// <summary>Retrieves a Ppe aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe?> GetByIdAsync(PpeId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Ppe aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe>> ListAsync(PpeSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Ppe aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Ppe aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe aggregate, CancellationToken cancellationToken = default);
}