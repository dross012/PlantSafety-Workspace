// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           StartAITrainingJobCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StartAITrainingJobCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AITraining.Interfaces;
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AITraining.Commands;

/// <summary>Handles <see cref="StartAITrainingJobCommand"/>.</summary>
public sealed class StartAITrainingJobCommandHandler : IRequestHandler<StartAITrainingJobCommand, Result<AITrainingJobStatusResult>>
{
    private readonly IAITrainingJobRepository _repository;
    private readonly ILogger<StartAITrainingJobCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartAITrainingJobCommandHandler(IAITrainingJobRepository repository, ILogger<StartAITrainingJobCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AITrainingJobStatusResult>> Handle(StartAITrainingJobCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AITrainingJobId.FromGuid(request.AITrainingJobId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AITrainingJobStatusResult>.Failure(new ApplicationError(
                    "Intelligence.AITraining.NotFound",
                    "The requested AI training job could not be found.",
                    request.AITrainingJobId,
                    ErrorSeverity.NotFound));
            }

            var startedBy = UserId.FromGuid(request.StartedByUserId);

            aggregate.Start(
                startedBy: startedBy,
                startedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(startedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AITrainingJobStatusResult>.Success(new AITrainingJobStatusResult(
                aggregate.AITrainingJobId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start AITrainingJob {AITrainingJobId}", request.AITrainingJobId);

            return Result<AITrainingJobStatusResult>.Failure(new ApplicationError(
                "Intelligence.AITraining.StartFailed",
                "Unable to start the AI training job at this time.",
                Guid.NewGuid()));
        }
    }
}
