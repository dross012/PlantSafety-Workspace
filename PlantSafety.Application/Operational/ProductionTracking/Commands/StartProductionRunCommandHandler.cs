// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           StartProductionRunCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for StartProductionRunCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Handles <see cref="StartProductionRunCommand"/>.</summary>
public sealed class StartProductionRunCommandHandler(IProductionRunRepository repository, ILogger<StartProductionRunCommandHandler> logger)
    : IRequestHandler<StartProductionRunCommand, Result<StartProductionRunResult>>
{
    /// <inheritdoc/>
    public async Task<Result<StartProductionRunResult>> Handle(StartProductionRunCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var aggregate = Domain.Operational.ProductionTracking.Aggregates.ProductionRun.Start(
                createdBy: requesterId,
                startedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                runCode: request.RunCode,
                lineId: ProductionLineId.FromGuid(request.LineId),
                lineName: request.LineName,
                machineId: request.MachineId,
                operatorName: request.OperatorName,
                idealCycleTimeSeconds: request.IdealCycleTimeSeconds);

            await repository.AddAsync(aggregate, cancellationToken);

            return Result<StartProductionRunResult>.Success(new StartProductionRunResult(
                aggregate.ProductionRunId.Value,
                aggregate.RunCode,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start ProductionRun for Tenant {TenantId}", request.TenantId);

            return Result<StartProductionRunResult>.Failure(new ApplicationError(
                "Operational.ProductionTracking.StartRunFailed",
                "Unable to start the production run at this time.",
                Guid.NewGuid()));
        }
    }
}
