// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordEffluentSampleCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Enums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="RecordEffluentSampleCommand"/>.</summary>
public sealed class RecordEffluentSampleCommandHandler : IRequestHandler<RecordEffluentSampleCommand, Result<EffluentSampleActionResult>>
{
    private readonly IEffluentSampleRepository _repository;
    private readonly ILogger<RecordEffluentSampleCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordEffluentSampleCommandHandler(IEffluentSampleRepository repository, ILogger<RecordEffluentSampleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EffluentSampleActionResult>> Handle(RecordEffluentSampleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var parameter = Enum.TryParse<SampleParameterType>(request.Parameter, true, out var parsedParameter) ? parsedParameter : SampleParameterType.Other;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                dischargePointId: request.DischargePointId,
                parameter: parameter,
                measuredValue: request.MeasuredValue,
                unitOfMeasure: request.UnitOfMeasure,
                sampledAtUtc: UtcTimestamp.From(request.SampledAtUtc),
                collectedByUserId: UserId.FromGuid(request.CollectedByUserId),
                labSourceSystem: request.LabSourceSystem,
                chainOfCustodyReference: request.ChainOfCustodyReference,
                correctionOfSampleId: request.CorrectionOfSampleId,
                correctionReason: request.CorrectionReason);

            await _repository.AddAsync(aggregate, cancellationToken);

            // Data-governance: chain-of-custody is immutable/append-only. When this record is a
            // correction, mark the original sample as Corrected without mutating any of its data.
            if (request.CorrectionOfSampleId is Guid originalId)
            {
                var original = await _repository.GetByIdAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.ValueObjects.Ids.EffluentSampleId.FromGuid(originalId), cancellationToken);
                if (original is not null)
                {
                    original.MarkCorrected(
                        aggregate.EffluentSampleId.Value,
                        request.CorrectionReason ?? string.Empty,
                        UtcTimestamp.From(request.RequestedAtUtc),
                        PerformedByActor.User(performedBy),
                        new CorrelationId(request.CorrelationId),
                        new CausationId(request.CausationId));

                    await _repository.UpdateAsync(original, cancellationToken);
                }
            }

            return Result<EffluentSampleActionResult>.Success(new EffluentSampleActionResult(
                aggregate.EffluentSampleId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record EffluentSample for tenant {TenantId}", request.TenantId);

            return Result<EffluentSampleActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.RecordFailed",
                "Unable to record the effluent sample at this time.",
                Guid.NewGuid()));
        }
    }
}