// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetDeadLetterEventsRecentQuery.
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

/// <summary>Handles <see cref="GetDeadLetterEventsRecentQuery"/>.</summary>
public sealed class GetDeadLetterEventsRecentQueryHandler
    : IRequestHandler<GetDeadLetterEventsRecentQuery, Result<IReadOnlyList<DeadLetterEventDto>>>
{
    private readonly IDeadLetterEventRepository _repository;
    private readonly ILogger<GetDeadLetterEventsRecentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDeadLetterEventsRecentQueryHandler(
        IDeadLetterEventRepository repository, ILogger<GetDeadLetterEventsRecentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DeadLetterEventDto>>> Handle(
        GetDeadLetterEventsRecentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRecentAsync(
                request.TenantId, request.EventType, request.IncludeReplayed, request.Top, request.Skip,
                cancellationToken);
            return Result<IReadOnlyList<DeadLetterEventDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve recent dead-lettered events.");

            return Result<IReadOnlyList<DeadLetterEventDto>>.Failure(new ApplicationError(
                "Messaging.DeadLetterEvents.RetrievalFailed",
                "Unable to retrieve recent dead-lettered events at this time.",
                Guid.NewGuid()));
        }
    }
}
