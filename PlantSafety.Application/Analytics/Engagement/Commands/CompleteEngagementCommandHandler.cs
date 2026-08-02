// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           CompleteEngagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteEngagementCommand.
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

/// <summary>Handles <see cref="CompleteEngagementCommand"/>.</summary>
public sealed class CompleteEngagementCommandHandler : IRequestHandler<CompleteEngagementCommand, Result<EngagementStatusResult>>
{
    private readonly IEngagementRepository _repository;
    private readonly ILogger<CompleteEngagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteEngagementCommandHandler(IEngagementRepository repository, ILogger<CompleteEngagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EngagementStatusResult>> Handle(CompleteEngagementCommand request, CancellationToken cancellationToken)
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

            aggregate.Complete(
                completedBy: performedBy,
                completedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to complete Engagement {EngagementId}", request.EngagementId);

            return Result<EngagementStatusResult>.Failure(new ApplicationError(
                "Analytics.Engagement.CompleteFailed",
                "Unable to complete the engagement measurement at this time.",
                Guid.NewGuid()));
        }
    }
}
