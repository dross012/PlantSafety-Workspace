// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           StartAnalysisRCACommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StartAnalysisRCACommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Handles <see cref="StartAnalysisRCACommand"/>.</summary>
public sealed class StartAnalysisRCACommandHandler : IRequestHandler<StartAnalysisRCACommand, Result<RCAStatusResult>>
{
    private readonly IRCARepository _repository;
    private readonly ILogger<StartAnalysisRCACommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartAnalysisRCACommandHandler(IRCARepository repository, ILogger<StartAnalysisRCACommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RCAStatusResult>> Handle(StartAnalysisRCACommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RCAId.FromGuid(request.RcaId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RCAStatusResult>.Failure(new ApplicationError(
                    "ContinuousImprovement.RCA.NotFound",
                    "The requested root cause analysis could not be found.",
                    request.RcaId,
                    ErrorSeverity.NotFound));
            }

            var startedBy = UserId.FromGuid(request.StartedByUserId);

            aggregate.StartAnalysis(
                startedBy: startedBy,
                startedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(startedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RCAStatusResult>.Success(new RCAStatusResult(
                aggregate.RcaId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start analysis for RCA {RcaId}", request.RcaId);

            return Result<RCAStatusResult>.Failure(new ApplicationError(
                "ContinuousImprovement.RCA.StartAnalysisFailed",
                "Unable to start analysis on the root cause analysis at this time.",
                Guid.NewGuid()));
        }
    }
}
