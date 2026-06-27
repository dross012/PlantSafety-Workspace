// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           ICommandHandler.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Framework-agnostic command handler contract.
//                  Application layer implements via MediatR IRequestHandler.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>Framework-agnostic command handler for commands without a return value.</summary>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>Framework-agnostic command handler for commands returning a typed result.</summary>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
