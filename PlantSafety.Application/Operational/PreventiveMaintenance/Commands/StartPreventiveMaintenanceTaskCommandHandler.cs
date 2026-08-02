// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           StartPreventiveMaintenanceTaskCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for StartPreventiveMaintenanceTaskCommand. GetByIdAsync is
//                  tenant-scoped, the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Handles <see cref="StartPreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class StartPreventiveMaintenanceTaskCommandHandler : IRequestHandler<StartPreventiveMaintenanceTaskCommand, Result<PreventiveMaintenanceTaskStatusResult>>
{
    private readonly IPreventiveMaintenanceTaskRepository _repository;
    private readonly ILogger<StartPreventiveMaintenanceTaskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartPreventiveMaintenanceTaskCommandHandler(IPreventiveMaintenanceTaskRepository repository, ILogger<StartPreventiveMaintenanceTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreventiveMaintenanceTaskStatusResult>> Handle(StartPreventiveMaintenanceTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PreventiveMaintenanceTaskId.FromGuid(request.PreventiveMaintenanceTaskId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PreventiveMaintenanceTaskStatusResult>.Failure(new ApplicationError(
                    "Operational.PreventiveMaintenance.NotFound",
                    "The requested preventive maintenance task could not be found.",
                    request.PreventiveMaintenanceTaskId,
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

            return Result<PreventiveMaintenanceTaskStatusResult>.Success(new PreventiveMaintenanceTaskStatusResult(
                aggregate.PreventiveMaintenanceTaskId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409 --
            // swallowing it here as a generic ApplicationError would misreport a stale-ETag
            // conflict as an ordinary 400 failure.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start PreventiveMaintenanceTask {PreventiveMaintenanceTaskId}", request.PreventiveMaintenanceTaskId);

            return Result<PreventiveMaintenanceTaskStatusResult>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.StartFailed",
                "Unable to start the preventive maintenance task at this time.",
                Guid.NewGuid()));
        }
    }
}
