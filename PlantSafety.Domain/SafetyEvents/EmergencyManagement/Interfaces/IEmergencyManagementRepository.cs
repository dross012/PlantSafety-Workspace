// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           IEmergencyManagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EmergencyManagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="EmergencyManagement"/> aggregate root.</summary>
public interface IEmergencyManagementRepository
{
    /// <summary>Retrieves a EmergencyManagement aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<EmergencyManagement?> GetByIdAsync(EmergencyManagementId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EmergencyManagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<EmergencyManagement>> ListAsync(EmergencyManagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EmergencyManagement aggregate.</summary>
    Task AddAsync(EmergencyManagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EmergencyManagement aggregate.</summary>
    Task UpdateAsync(EmergencyManagement aggregate, CancellationToken cancellationToken = default);
}
