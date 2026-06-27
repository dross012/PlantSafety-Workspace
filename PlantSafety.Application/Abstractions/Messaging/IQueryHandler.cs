// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical handler interface for Queries in the CQRS pipeline.
//   Query handlers are read-only orchestrators; they call repository read methods only.
//   They must never raise domain events, participate in transactions, or mutate state.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no signatures may be changed.

using MediatR;

namespace PlantSafety.Application.Abstractions.Messaging;

/// <summary>
/// Handler contract for read-only queries.
/// Implementors MUST: call only read-side repository methods, return mapped DTOs (never domain objects),
/// log structured SIEM-safe envelopes, and propagate correlation lineage.
/// </summary>
/// <typeparam name="TQuery">The query type implementing <see cref="IQuery{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The strongly typed read model produced by the handler.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
