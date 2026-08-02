// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           ArchiveAITrainingJobCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveAITrainingJobCommand.
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

/// <summary>Handles <see cref="ArchiveAITrainingJobCommand"/>.</summary>
public sealed class ArchiveAITrainingJobCommandHandler : IRequestHandler<ArchiveAITrainingJobCommand, Result<AITrainingJobStatusResult>>
{
    private readonly IAITrainingJobRepository _repository;
    private readonly ILogger<ArchiveAITrainingJobCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveAITrainingJobCommandHandler(IAITrainingJobRepository repository, ILogger<ArchiveAITrainingJobCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AITrainingJobStatusResult>> Handle(ArchiveAITrainingJobCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
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
            _logger.LogError(ex, "Failed to archive AITrainingJob {AITrainingJobId}", request.AITrainingJobId);

            return Result<AITrainingJobStatusResult>.Failure(new ApplicationError(
                "Intelligence.AITraining.ArchiveFailed",
                "Unable to archive the AI training job at this time.",
                Guid.NewGuid()));
        }
    }
}
