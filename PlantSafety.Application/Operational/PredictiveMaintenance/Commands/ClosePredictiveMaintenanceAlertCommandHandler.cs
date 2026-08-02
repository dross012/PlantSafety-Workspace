// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           ClosePredictiveMaintenanceAlertCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ClosePredictiveMaintenanceAlertCommand. Mirrors
//                  ReviewPredictiveMaintenanceAlertCommandHandler's conventions.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Handles <see cref="ClosePredictiveMaintenanceAlertCommand"/>.</summary>
public sealed class ClosePredictiveMaintenanceAlertCommandHandler : IRequestHandler<ClosePredictiveMaintenanceAlertCommand, Result<PredictiveMaintenanceAlertStatusResult>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<ClosePredictiveMaintenanceAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ClosePredictiveMaintenanceAlertCommandHandler(IPredictiveMaintenanceAlertRepository repository, ILogger<ClosePredictiveMaintenanceAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceAlertStatusResult>> Handle(ClosePredictiveMaintenanceAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PredictiveMaintenanceAlertId.FromGuid(request.PredictiveMaintenanceAlertId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.NotFound",
                    "The requested predictive maintenance alert could not be found.",
                    request.PredictiveMaintenanceAlertId,
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

            return Result<PredictiveMaintenanceAlertStatusResult>.Success(new PredictiveMaintenanceAlertStatusResult(
                aggregate.PredictiveMaintenanceAlertId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId}", request.PredictiveMaintenanceAlertId);

            return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.CloseFailed",
                "Unable to close the predictive maintenance alert at this time.",
                Guid.NewGuid()));
        }
    }
}
