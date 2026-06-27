// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IUnitOfWork.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit of Work contract. Scopes all repository writes to a single
//                  atomic transaction and dispatches domain events after commit.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>
/// Unit of Work contract.
/// Coordinates all repository mutations within a single atomic DB transaction
/// and dispatches collected domain events after a successful commit.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending changes atomically.
    /// Domain events are dispatched after a successful commit.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
