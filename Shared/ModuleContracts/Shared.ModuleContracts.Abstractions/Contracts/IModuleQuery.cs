// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Contracts
//  File:           IModuleQuery.cs
//  Created:        2026-08-09
//  Description:    A query is a request to read data owned by one module,
//                  dispatched through IModuleQueryHandler -- this is the
//                  "query gateway" the spec asks cross-module calls to be
//                  replaced with, when module B needs data module A owns.
// ============================================================================
namespace Shared.ModuleContracts.Abstractions.Contracts;

/// <summary>Marker for a query: a request to read data owned by one module.</summary>
public interface IModuleQuery<out TResult>;

/// <summary>Handles exactly one query type.</summary>
public interface IModuleQueryHandler<in TQuery, TResult> where TQuery : IModuleQuery<TResult>
{
    /// <summary>Handles the query.</summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
