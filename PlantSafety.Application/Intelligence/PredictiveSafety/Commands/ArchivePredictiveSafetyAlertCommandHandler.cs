// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           ArchivePredictiveSafetyAlertCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchivePredictiveSafetyAlertCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces;
using PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Commands;

/// <summary>Handles <see cref="ArchivePredictiveSafetyAlertCommand"/>.</summary>
public sealed class ArchivePredictiveSafetyAlertCommandHandler : IRequestHandler<ArchivePredictiveSafetyAlertCommand, Result<PredictiveSafetyAlertStatusResult>>
{
    private readonly IPredictiveSafetyAlertRepository _repository;
    private readonly ILogger<ArchivePredictiveSafetyAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchivePredictiveSafetyAlertCommandHandler(IPredictiveSafetyAlertRepository repository, ILogger<ArchivePredictiveSafetyAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveSafetyAlertStatusResult>> Handle(ArchivePredictiveSafetyAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PredictiveSafetyAlertId.FromGuid(request.PredictiveSafetyAlertId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PredictiveSafetyAlertStatusResult>.Failure(new ApplicationError(
                    "Intelligence.PredictiveSafety.NotFound",
                    "The requested predictive safety alert could not be found.",
                    request.PredictiveSafetyAlertId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PredictiveSafetyAlertStatusResult>.Success(new PredictiveSafetyAlertStatusResult(
                aggregate.PredictiveSafetyAlertId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive PredictiveSafetyAlert {PredictiveSafetyAlertId}", request.PredictiveSafetyAlertId);

            return Result<PredictiveSafetyAlertStatusResult>.Failure(new ApplicationError(
                "Intelligence.PredictiveSafety.ArchiveFailed",
                "Unable to archive the predictive safety alert at this time.",
                Guid.NewGuid()));
        }
    }
}
