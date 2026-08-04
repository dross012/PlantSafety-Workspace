// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DefineDischargeLimitCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Enums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="DefineDischargeLimitCommand"/>.</summary>
public sealed class DefineDischargeLimitCommandHandler : IRequestHandler<DefineDischargeLimitCommand, Result<DischargeLimitActionResult>>
{
    private readonly IDischargeLimitRepository _repository;
    private readonly ILogger<DefineDischargeLimitCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DefineDischargeLimitCommandHandler(IDischargeLimitRepository repository, ILogger<DefineDischargeLimitCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargeLimitActionResult>> Handle(DefineDischargeLimitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var parameter = Enum.TryParse<DischargeParameterType>(request.Parameter, true, out var parsedParameter) ? parsedParameter : DischargeParameterType.Other;
            var basis = Enum.TryParse<LimitBasisType>(request.LimitBasis, true, out var parsedBasis) ? parsedBasis : LimitBasisType.DailyMaximum;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                dischargePointId: request.DischargePointId,
                dischargePermitId: request.DischargePermitId,
                parameter: parameter,
                limitBasis: basis,
                limitValue: request.LimitValue,
                unitOfMeasure: request.UnitOfMeasure,
                effectiveAtUtc: UtcTimestamp.From(request.EffectiveAtUtc),
                permitConditionReference: request.PermitConditionReference);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DischargeLimitActionResult>.Success(new DischargeLimitActionResult(
                aggregate.DischargeLimitId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define DischargeLimit for tenant {TenantId}", request.TenantId);

            return Result<DischargeLimitActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.DefineFailed",
                "Unable to define the discharge limit at this time.",
                Guid.NewGuid()));
        }
    }
}