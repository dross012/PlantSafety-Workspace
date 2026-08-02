// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           VerifyPreventiveMaintenanceTaskCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for VerifyPreventiveMaintenanceTaskCommand. Mirrors
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

/// <summary>Handles <see cref="VerifyPreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class VerifyPreventiveMaintenanceTaskCommandHandler : IRequestHandler<VerifyPreventiveMaintenanceTaskCommand, Result<PreventiveMaintenanceTaskStatusResult>>
{
    private readonly IPreventiveMaintenanceTaskRepository _repository;
    private readonly ILogger<VerifyPreventiveMaintenanceTaskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VerifyPreventiveMaintenanceTaskCommandHandler(IPreventiveMaintenanceTaskRepository repository, ILogger<VerifyPreventiveMaintenanceTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreventiveMaintenanceTaskStatusResult>> Handle(VerifyPreventiveMaintenanceTaskCommand request, CancellationToken cancellationToken)
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

            var verifiedBy = UserId.FromGuid(request.VerifiedByUserId);

            aggregate.Verify(
                verifiedBy: verifiedBy,
                verifiedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(verifiedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PreventiveMaintenanceTaskStatusResult>.Success(new PreventiveMaintenanceTaskStatusResult(
                aggregate.PreventiveMaintenanceTaskId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify PreventiveMaintenanceTask {PreventiveMaintenanceTaskId}", request.PreventiveMaintenanceTaskId);

            return Result<PreventiveMaintenanceTaskStatusResult>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.VerifyFailed",
                "Unable to verify the preventive maintenance task at this time.",
                Guid.NewGuid()));
        }
    }
}
