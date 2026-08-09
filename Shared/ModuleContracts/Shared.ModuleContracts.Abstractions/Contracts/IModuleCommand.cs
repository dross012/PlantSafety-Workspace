// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Contracts
//  File:           IModuleCommand.cs
//  Created:        2026-08-09
//  Description:    Phase 2.7 (Module Boundary Stabilization): module
//                  boundary contracts. A command is an intent to change
//                  state within exactly one module, dispatched through
//                  IModuleCommandHandler -- never a direct method call
//                  into another module's internals.
// ============================================================================
namespace Shared.ModuleContracts.Abstractions.Contracts;

/// <summary>Marker for a command: an intent to change state within one module.</summary>
public interface IModuleCommand<out TResult>;

/// <summary>Handles exactly one command type.</summary>
public interface IModuleCommandHandler<in TCommand, TResult> where TCommand : IModuleCommand<TResult>
{
    /// <summary>Handles the command.</summary>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
