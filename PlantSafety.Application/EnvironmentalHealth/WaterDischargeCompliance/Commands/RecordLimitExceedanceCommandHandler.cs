// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordLimitExceedanceCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Enums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="RecordLimitExceedanceCommand"/>.</summary>
public sealed class RecordLimitExceedanceCommandHandler : IRequestHandler<RecordLimitExceedanceCommand, Result<LimitExceedanceActionResult>>
{
    private readonly ILimitExceedanceRepository _repository;
    private readonly ILogger<RecordLimitExceedanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordLimitExceedanceCommandHandler(ILimitExceedanceRepository repository, ILogger<RecordLimitExceedanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LimitExceedanceActionResult>> Handle(RecordLimitExceedanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var severity = Enum.TryParse<ExceedanceSeverity>(request.Severity, true, out var parsedSeverity) ? parsedSeverity : ExceedanceSeverity.Minor;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                dischargePointId: request.DischargePointId,
                effluentSampleId: request.EffluentSampleId,
                dischargeLimitId: request.DischargeLimitId,
                parameterName: request.ParameterName,
                measuredValue: request.MeasuredValue,
                limitValue: request.LimitValue,
                severity: severity,
                detectedAtUtc: UtcTimestamp.From(request.DetectedAtUtc),
                repeatExceedanceCount: request.RepeatExceedanceCount);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<LimitExceedanceActionResult>.Success(new LimitExceedanceActionResult(
                aggregate.LimitExceedanceId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record LimitExceedance for tenant {TenantId}", request.TenantId);

            return Result<LimitExceedanceActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.RecordFailed",
                "Unable to record the limit exceedance at this time.",
                Guid.NewGuid()));
        }
    }
}