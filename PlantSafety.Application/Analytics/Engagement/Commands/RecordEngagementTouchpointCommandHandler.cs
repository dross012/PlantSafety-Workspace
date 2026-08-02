// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           RecordEngagementTouchpointCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordEngagementTouchpointCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.Engagement.Interfaces;
using PlantSafety.Domain.Analytics.Engagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Engagement.Commands;

/// <summary>Handles <see cref="RecordEngagementTouchpointCommand"/>.</summary>
public sealed class RecordEngagementTouchpointCommandHandler : IRequestHandler<RecordEngagementTouchpointCommand, Result<EngagementStatusResult>>
{
    private readonly IEngagementRepository _repository;
    private readonly ILogger<RecordEngagementTouchpointCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordEngagementTouchpointCommandHandler(IEngagementRepository repository, ILogger<RecordEngagementTouchpointCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EngagementStatusResult>> Handle(RecordEngagementTouchpointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EngagementId.FromGuid(request.EngagementId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EngagementStatusResult>.Failure(new ApplicationError(
                    "Analytics.Engagement.NotFound",
                    "The requested engagement record could not be found.",
                    request.EngagementId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordTouchpoint(
                touchedAtUtc: UtcTimestamp.Now(),
                responseCount: request.ResponseCount,
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EngagementStatusResult>.Success(new EngagementStatusResult(
                aggregate.EngagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record touchpoint for Engagement {EngagementId}", request.EngagementId);

            return Result<EngagementStatusResult>.Failure(new ApplicationError(
                "Analytics.Engagement.RecordTouchpointFailed",
                "Unable to record the engagement touchpoint at this time.",
                Guid.NewGuid()));
        }
    }
}
