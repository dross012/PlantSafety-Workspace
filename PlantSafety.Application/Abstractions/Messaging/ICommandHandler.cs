// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical handler interfaces for Commands in the CQRS pipeline.
//   Handlers contain ONLY orchestration logic. No domain logic, no SQL, no HTTP.
//   All handlers must be async and accept CancellationToken.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no signatures may be changed.

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Abstractions.Messaging;

/// <summary>
/// Handler contract for commands that return a typed result.
/// Implementors MUST: orchestrate via repositories, map deterministically,
/// log structured SIEM-safe envelopes, propagate correlation lineage, and wrap exceptions.
/// </summary>
/// <typeparam name="TCommand">The command type implementing <see cref="ICommand{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The strongly typed result produced by the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Handler contract for commands that produce no typed result.
/// </summary>
/// <typeparam name="TCommand">The command type implementing <see cref="ICommand"/>.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}
