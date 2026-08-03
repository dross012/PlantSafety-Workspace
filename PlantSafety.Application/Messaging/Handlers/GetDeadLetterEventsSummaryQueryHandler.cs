// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetDeadLetterEventsSummaryQuery.
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

/// <summary>Handles <see cref="GetDeadLetterEventsSummaryQuery"/>.</summary>
public sealed class GetDeadLetterEventsSummaryQueryHandler
    : IRequestHandler<GetDeadLetterEventsSummaryQuery, Result<IReadOnlyList<DeadLetterEventsSummaryDto>>>
{
    private readonly IDeadLetterEventRepository _repository;
    private readonly ILogger<GetDeadLetterEventsSummaryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDeadLetterEventsSummaryQueryHandler(
        IDeadLetterEventRepository repository, ILogger<GetDeadLetterEventsSummaryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DeadLetterEventsSummaryDto>>> Handle(
        GetDeadLetterEventsSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSummaryAsync(request.TenantId, request.LookbackDays, cancellationToken);
            return Result<IReadOnlyList<DeadLetterEventsSummaryDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve dead-letter summary.");

            return Result<IReadOnlyList<DeadLetterEventsSummaryDto>>.Failure(new ApplicationError(
                "Messaging.DeadLetterEvents.SummaryRetrievalFailed",
                "Unable to retrieve the dead-letter summary at this time.",
                Guid.NewGuid()));
        }
    }
}
