// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           ArchiveRootCauseAnalysisCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveRootCauseAnalysisCommand.
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

/// <summary>Handles <see cref="ArchiveRootCauseAnalysisCommand"/>.</summary>
public sealed class ArchiveRootCauseAnalysisCommandHandler : IRequestHandler<ArchiveRootCauseAnalysisCommand, Result<RootCauseAnalysisStatusResult>>
{
    private readonly IRootCauseAnalysisRepository _repository;
    private readonly ILogger<ArchiveRootCauseAnalysisCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveRootCauseAnalysisCommandHandler(IRootCauseAnalysisRepository repository, ILogger<ArchiveRootCauseAnalysisCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RootCauseAnalysisStatusResult>> Handle(ArchiveRootCauseAnalysisCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
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
            _logger.LogError(ex, "Failed to archive RootCauseAnalysis {RootCauseAnalysisId}", request.RootCauseAnalysisId);

            return Result<RootCauseAnalysisStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.RootCauseAnalysis.ArchiveFailed",
                "Unable to archive the root cause analysis at this time.",
                Guid.NewGuid()));
        }
    }
}
