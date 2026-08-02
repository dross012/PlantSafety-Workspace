// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           CloseRootCauseAnalysisCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseRootCauseAnalysisCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;

/// <summary>Handles <see cref="CloseRootCauseAnalysisCommand"/>.</summary>
public sealed class CloseRootCauseAnalysisCommandHandler : IRequestHandler<CloseRootCauseAnalysisCommand, Result<RootCauseAnalysisStatusResult>>
{
    private readonly IRootCauseAnalysisRepository _repository;
    private readonly ILogger<CloseRootCauseAnalysisCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseRootCauseAnalysisCommandHandler(IRootCauseAnalysisRepository repository, ILogger<CloseRootCauseAnalysisCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RootCauseAnalysisStatusResult>> Handle(CloseRootCauseAnalysisCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RootCauseAnalysisId.FromGuid(request.RootCauseAnalysisId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RootCauseAnalysisStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.RootCauseAnalysis.NotFound",
                    "The requested root cause analysis could not be found.",
                    request.RootCauseAnalysisId,
                    ErrorSeverity.NotFound));
            }

            var closedBy = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
                closedBy: closedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(closedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RootCauseAnalysisStatusResult>.Success(new RootCauseAnalysisStatusResult(
                aggregate.RootCauseAnalysisId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close RootCauseAnalysis {RootCauseAnalysisId}", request.RootCauseAnalysisId);

            return Result<RootCauseAnalysisStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.RootCauseAnalysis.CloseFailed",
                "Unable to close the root cause analysis at this time.",
                Guid.NewGuid()));
        }
    }
}
