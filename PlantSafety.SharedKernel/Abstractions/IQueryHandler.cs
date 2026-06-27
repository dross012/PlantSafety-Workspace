// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IQueryHandler.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Framework-agnostic query handler contract.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>Framework-agnostic query handler for read-only operations.</summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
