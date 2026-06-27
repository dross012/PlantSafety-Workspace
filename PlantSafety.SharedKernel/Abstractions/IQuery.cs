// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IQuery.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Framework-agnostic marker for all CQRS queries.
//                  Queries MUST be side-effect free and read-only.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>Framework-agnostic marker for CQRS queries returning a typed result.</summary>
/// <typeparam name="TResult">The result type returned by this query.</typeparam>
public interface IQuery<out TResult> { }
