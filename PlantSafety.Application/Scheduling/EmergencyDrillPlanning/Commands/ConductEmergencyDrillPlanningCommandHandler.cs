// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           ConductEmergencyDrillPlanningCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ConductEmergencyDrillPlanningCommand. GetByIdAsync is tenant-scoped,
//                  the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one (this aggregate had no write path to retrofit). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Handles <see cref="ConductEmergencyDrillPlanningCommand"/>.</summary>
public sealed class ConductEmergencyDrillPlanningCommandHandler : IRequestHandler<ConductEmergencyDrillPlanningCommand, Result<EmergencyDrillPlanningStatusResult>>
{
    private readonly IEmergencyDrillPlanningRepository _repository;
    private readonly ILogger<ConductEmergencyDrillPlanningCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ConductEmergencyDrillPlanningCommandHandler(IEmergencyDrillPlanningRepository repository, ILogger<ConductEmergencyDrillPlanningCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyDrillPlanningStatusResult>> Handle(ConductEmergencyDrillPlanningCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EmergencyDrillPlanningId.FromGuid(request.EmergencyDrillPlanningId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EmergencyDrillPlanningStatusResult>.Failure(new ApplicationError(
                    "Scheduling.EmergencyDrillPlanning.NotFound",
                    "The requested emergency drill could not be found.",
                    request.EmergencyDrillPlanningId,
                    ErrorSeverity.NotFound));
            }

            var conductedBy = UserId.FromGuid(request.ConductedByUserId);

            aggregate.Conduct(
                conductedBy: conductedBy,
                participationCount: request.ParticipationCount,
                conductedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(conductedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EmergencyDrillPlanningStatusResult>.Success(new EmergencyDrillPlanningStatusResult(
                aggregate.EmergencyDrillPlanningId.Value,
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
            _logger.LogError(ex, "Failed to conduct EmergencyDrillPlanning {EmergencyDrillPlanningId}", request.EmergencyDrillPlanningId);

            return Result<EmergencyDrillPlanningStatusResult>.Failure(new ApplicationError(
                "Scheduling.EmergencyDrillPlanning.ConductFailed",
                "Unable to record the drill as conducted at this time.",
                Guid.NewGuid()));
        }
    }
}
