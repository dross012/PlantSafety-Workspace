// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           IControlChangeEventRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ControlChangeEvent aggregate root.
//                  AddAsync is the only write operation -- records are append-only.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.ControlChangeEvent"/> aggregate root.</summary>
public interface IControlChangeEventRepository
{
    /// <summary>Retrieves a ControlChangeEvent by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.ControlChangeEvent?> GetByIdAsync(ControlChangeEventId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recently recorded ControlChangeEvent for the given asset, used to chain <c>PriorRecordHash</c>.</summary>
    Task<Aggregates.ControlChangeEvent?> GetLatestForAssetAsync(OtAssetId otAssetId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns ControlChangeEvents matching the given authorization status.</summary>
    Task<IReadOnlyList<Aggregates.ControlChangeEvent>> ListByAuthorizationStatusAsync(Guid tenantId, ControlChangeAuthorizationStatus status, CancellationToken cancellationToken = default);

    /// <summary>Appends a new, immutable ControlChangeEvent record.</summary>
    Task AddAsync(Aggregates.ControlChangeEvent aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists an authorization-status reclassification for an existing record.</summary>
    Task UpdateAsync(Aggregates.ControlChangeEvent aggregate, CancellationToken cancellationToken = default);
}
