// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           ScheduleEmergencyDrillPlanningCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ScheduleEmergencyDrillPlanningCommand. Pure orchestration:
//                  constructs the EmergencyDrillPlanning aggregate via its domain factory method and
//                  persists it through the domain repository interface. No business rules live
//                  here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Handles <see cref="ScheduleEmergencyDrillPlanningCommand"/>.</summary>
public sealed class ScheduleEmergencyDrillPlanningCommandHandler : IRequestHandler<ScheduleEmergencyDrillPlanningCommand, Result<ScheduleEmergencyDrillPlanningResult>>
{
    private readonly IEmergencyDrillPlanningRepository _repository;
    private readonly ILogger<ScheduleEmergencyDrillPlanningCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleEmergencyDrillPlanningCommandHandler(IEmergencyDrillPlanningRepository repository, ILogger<ScheduleEmergencyDrillPlanningCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScheduleEmergencyDrillPlanningResult>> Handle(ScheduleEmergencyDrillPlanningCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduledBy = UserId.FromGuid(request.ScheduledByUserId);

            var aggregate = PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning.Create(
                createdBy: scheduledBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(scheduledBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                drillType: request.DrillType,
                shiftCode: request.ShiftCode,
                scheduledAtUtc: request.ScheduledAtUtc.HasValue ? UtcTimestamp.From(request.ScheduledAtUtc.Value) : null,
                participationCount: request.ParticipationCount);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<ScheduleEmergencyDrillPlanningResult>.Success(new ScheduleEmergencyDrillPlanningResult(
                aggregate.EmergencyDrillPlanningId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule EmergencyDrillPlanning for Tenant {TenantId}", request.TenantId);

            return Result<ScheduleEmergencyDrillPlanningResult>.Failure(new ApplicationError(
                "Scheduling.EmergencyDrillPlanning.CreateFailed",
                "Unable to schedule the emergency drill at this time.",
                Guid.NewGuid()));
        }
    }
}
