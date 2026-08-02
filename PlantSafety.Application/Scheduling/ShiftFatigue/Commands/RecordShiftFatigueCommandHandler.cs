// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           RecordShiftFatigueCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordShiftFatigueCommand. Pure orchestration: constructs the
//                  ShiftFatigue aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Commands;

/// <summary>Handles <see cref="RecordShiftFatigueCommand"/>.</summary>
public sealed class RecordShiftFatigueCommandHandler : IRequestHandler<RecordShiftFatigueCommand, Result<RecordShiftFatigueResult>>
{
    private readonly IShiftFatigueRepository _repository;
    private readonly ILogger<RecordShiftFatigueCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordShiftFatigueCommandHandler(IShiftFatigueRepository repository, ILogger<RecordShiftFatigueCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordShiftFatigueResult>> Handle(RecordShiftFatigueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                employeeId: request.EmployeeId,
                shiftPattern: request.ShiftPattern,
                consecutiveNightShifts: request.ConsecutiveNightShifts,
                overtimeHours: request.OvertimeHours,
                fatigueRiskScore: request.FatigueRiskScore);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RecordShiftFatigueResult>.Success(new RecordShiftFatigueResult(
                aggregate.ShiftFatigueId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record ShiftFatigue for Tenant {TenantId}", request.TenantId);

            return Result<RecordShiftFatigueResult>.Failure(new ApplicationError(
                "Scheduling.ShiftFatigue.CreateFailed",
                "Unable to record the fatigue assessment at this time.",
                Guid.NewGuid()));
        }
    }
}
