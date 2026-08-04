// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RunSensorIntegrityCheckCommand. Computes the next recurring
//   check-due timestamp via SensorIntegrityRules and persists via the domain repository.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Rules;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Commands;

/// <summary>Handles <see cref="RunSensorIntegrityCheckCommand"/>.</summary>
public sealed class RunSensorIntegrityCheckCommandHandler : IRequestHandler<RunSensorIntegrityCheckCommand, Result<RunSensorIntegrityCheckResult>>
{
    private readonly ISensorIntegrityCheckRepository _repository;
    private readonly ILogger<RunSensorIntegrityCheckCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RunSensorIntegrityCheckCommandHandler(ISensorIntegrityCheckRepository repository, ILogger<RunSensorIntegrityCheckCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RunSensorIntegrityCheckResult>> Handle(RunSensorIntegrityCheckCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);

            if (!Enum.TryParse<SensorIntegrityCheckType>(request.CheckType, ignoreCase: true, out var checkType))
                checkType = SensorIntegrityCheckType.Signal;

            if (!Enum.TryParse<SensorIntegrityCheckResult>(request.Result, ignoreCase: true, out var result))
                result = SensorIntegrityCheckResult.Inconclusive;

            var now = UtcTimestamp.Now();
            var nextDue = UtcTimestamp.From(SensorIntegrityRules.ComputeNextCheckDueUtc(now.Value));

            var aggregate = PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Aggregates.SensorIntegrityCheck.Run(
                createdBy: actorId,
                createdAtUtc: now,
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                otAssetId: OtAssetId.FromGuid(request.OtAssetId),
                checkType: checkType,
                result: result,
                details: request.Details,
                checkedAtUtc: now,
                nextCheckDueUtc: nextDue);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RunSensorIntegrityCheckResult>.Success(new RunSensorIntegrityCheckResult(
                aggregate.SensorIntegrityCheckId.Value,
                aggregate.Result.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record SensorIntegrityCheck for Tenant {TenantId}", request.TenantId);

            return Result<RunSensorIntegrityCheckResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.SensorIntegrityCheck.RunFailed",
                "Unable to record the sensor-integrity check at this time.",
                Guid.NewGuid()));
        }
    }
}
