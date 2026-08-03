// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetDeadLetterEventsByCorrelationIdQuery.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Models;
using PlantSafety.Application.Messaging.Queries;
using PlantSafety.Application.Messaging.Repositories;

namespace PlantSafety.Application.Messaging.Handlers;

/// <summary>Handles <see cref="GetDeadLetterEventsByCorrelationIdQuery"/>.</summary>
public sealed class GetDeadLetterEventsByCorrelationIdQueryHandler
    : IRequestHandler<GetDeadLetterEventsByCorrelationIdQuery, Result<IReadOnlyList<DeadLetterEventDto>>>
{
    private readonly IDeadLetterEventRepository _repository;
    private readonly ILogger<GetDeadLetterEventsByCorrelationIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDeadLetterEventsByCorrelationIdQueryHandler(
        IDeadLetterEventRepository repository, ILogger<GetDeadLetterEventsByCorrelationIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DeadLetterEventDto>>> Handle(
        GetDeadLetterEventsByCorrelationIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
            return Result<IReadOnlyList<DeadLetterEventDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve dead-lettered events for CorrelationId {CorrelationId}.", request.CorrelationId);

            return Result<IReadOnlyList<DeadLetterEventDto>>.Failure(new ApplicationError(
                "Messaging.DeadLetterEvents.CorrelationTraceFailed",
                "Unable to trace dead-lettered events for this correlation at this time.",
                request.CorrelationId));
        }
    }
}
