// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           ScheduleHazardIsolationSequencingCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ScheduleHazardIsolationSequencingCommand. Pure orchestration:
//                  constructs the HazardIsolationSequencing aggregate via its domain factory method
//                  and persists it through the domain repository interface. No business rules live
//                  here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Commands;

/// <summary>Handles <see cref="ScheduleHazardIsolationSequencingCommand"/>.</summary>
public sealed class ScheduleHazardIsolationSequencingCommandHandler : IRequestHandler<ScheduleHazardIsolationSequencingCommand, Result<ScheduleHazardIsolationSequencingResult>>
{
    private readonly IHazardIsolationSequencingRepository _repository;
    private readonly ILogger<ScheduleHazardIsolationSequencingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleHazardIsolationSequencingCommandHandler(IHazardIsolationSequencingRepository repository, ILogger<ScheduleHazardIsolationSequencingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScheduleHazardIsolationSequencingResult>> Handle(ScheduleHazardIsolationSequencingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                hazardZone: request.HazardZone,
                taskType: request.TaskType,
                windowStartUtc: request.WindowStartUtc.HasValue ? UtcTimestamp.From(request.WindowStartUtc.Value) : null,
                windowEndUtc: request.WindowEndUtc.HasValue ? UtcTimestamp.From(request.WindowEndUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<ScheduleHazardIsolationSequencingResult>.Success(new ScheduleHazardIsolationSequencingResult(
                aggregate.HazardIsolationSequencingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule HazardIsolationSequencing for Tenant {TenantId}", request.TenantId);

            return Result<ScheduleHazardIsolationSequencingResult>.Failure(new ApplicationError(
                "Scheduling.HazardIsolationSequencing.CreateFailed",
                "Unable to schedule the hazard isolation window at this time.",
                Guid.NewGuid()));
        }
    }
}
