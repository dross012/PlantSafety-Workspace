// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to trace every dead-lettered event within a single failed cross-module
//   correlation chain.
// SchemaVersion: 1

using System;
using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Models;

namespace PlantSafety.Application.Messaging.Queries;

/// <summary>Retrieves every dead-lettered event sharing <paramref name="CorrelationId"/>, oldest first.</summary>
/// <param name="CorrelationId">The correlation identifier to trace.</param>
public sealed record GetDeadLetterEventsByCorrelationIdQuery(Guid CorrelationId)
    : IRequest<Result<IReadOnlyList<DeadLetterEventDto>>>;
