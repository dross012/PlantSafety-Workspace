// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           ICommand.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Framework-agnostic marker interface for all CQRS commands.
//                  SharedKernel MUST NOT reference MediatR directly.
//                  Application layer bridges this with IRequest<T>. Additive-only.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>Framework-agnostic marker for CQRS commands (no return value).</summary>
public interface ICommand { }

/// <summary>Framework-agnostic marker for CQRS commands returning a typed result.</summary>
/// <typeparam name="TResult">The result type produced by this command.</typeparam>
public interface ICommand<out TResult> { }
