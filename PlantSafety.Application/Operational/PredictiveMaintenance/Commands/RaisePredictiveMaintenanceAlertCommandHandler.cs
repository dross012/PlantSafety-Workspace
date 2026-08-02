// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           RaisePredictiveMaintenanceAlertCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for RaisePredictiveMaintenanceAlertCommand. Pure orchestration:
//                  constructs the PredictiveMaintenanceAlert aggregate via its domain factory
//                  method and persists it through the domain repository interface. No business
//                  rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Handles <see cref="RaisePredictiveMaintenanceAlertCommand"/>.</summary>
public sealed class RaisePredictiveMaintenanceAlertCommandHandler : IRequestHandler<RaisePredictiveMaintenanceAlertCommand, Result<PredictiveMaintenanceAlertStatusResult>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<RaisePredictiveMaintenanceAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RaisePredictiveMaintenanceAlertCommandHandler(IPredictiveMaintenanceAlertRepository repository, ILogger<RaisePredictiveMaintenanceAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceAlertStatusResult>> Handle(RaisePredictiveMaintenanceAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var raisedBy = UserId.FromGuid(request.RaisedByUserId);

            var aggregate = PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates.PredictiveMaintenanceAlert.Create(
                createdBy: raisedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(raisedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                equipmentId: request.EquipmentId,
                predictedFailureType: request.PredictedFailureType,
                predictedAtUtc: UtcTimestamp.From(request.PredictedAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<PredictiveMaintenanceAlertStatusResult>.Success(new PredictiveMaintenanceAlertStatusResult(
                aggregate.PredictiveMaintenanceAlertId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to raise PredictiveMaintenanceAlert for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.RaiseFailed",
                "Unable to raise the predictive maintenance alert at this time.",
                Guid.NewGuid()));
        }
    }
}
