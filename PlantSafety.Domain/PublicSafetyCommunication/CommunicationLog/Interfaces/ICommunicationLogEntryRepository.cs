// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           ICommunicationLogEntryRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the CommunicationLogEntry aggregate root.
//                  Append-only: no update method is exposed, consistent with the aggregate's
//                  immutability-once-recorded data-governance policy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;

/// <summary>Append-only domain repository interface for the <see cref="CommunicationLogEntry"/> aggregate root.</summary>
public interface ICommunicationLogEntryRepository
{
    /// <summary>Retrieves a CommunicationLogEntry aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry?> GetByIdAsync(CommunicationLogEntryId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all CommunicationLogEntry aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry>> ListAsync(CommunicationLogEntrySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new, immutable CommunicationLogEntry aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry aggregate, CancellationToken cancellationToken = default);
}
