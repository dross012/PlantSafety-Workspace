// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           ScheduleDrillsAndExercisesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ScheduleDrillsAndExercisesCommand. Pure orchestration: constructs
//                  the DrillsAndExercises aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;

/// <summary>Handles <see cref="ScheduleDrillsAndExercisesCommand"/>.</summary>
public sealed class ScheduleDrillsAndExercisesCommandHandler : IRequestHandler<ScheduleDrillsAndExercisesCommand, Result<DrillsAndExercisesStatusResult>>
{
    private readonly IDrillsAndExercisesRepository _repository;
    private readonly ILogger<ScheduleDrillsAndExercisesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleDrillsAndExercisesCommandHandler(IDrillsAndExercisesRepository repository, ILogger<ScheduleDrillsAndExercisesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DrillsAndExercisesStatusResult>> Handle(ScheduleDrillsAndExercisesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduledBy = UserId.FromGuid(request.ScheduledByUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates.DrillsAndExercises.Create(
                createdBy: scheduledBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(scheduledBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                drillType: request.DrillType,
                scheduledAtUtc: request.ScheduledAtUtc.HasValue ? UtcTimestamp.From(request.ScheduledAtUtc.Value) : (UtcTimestamp?)null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DrillsAndExercisesStatusResult>.Success(new DrillsAndExercisesStatusResult(
                aggregate.DrillsAndExercisesId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule DrillsAndExercises drill for Tenant {TenantId}", request.TenantId);

            return Result<DrillsAndExercisesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.DrillsAndExercises.ScheduleFailed",
                "Unable to schedule the drill at this time.",
                Guid.NewGuid()));
        }
    }
}
