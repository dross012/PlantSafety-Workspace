// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Vendor
//  Layer:          Application
//  File:           VendorPredictiveMaintenanceInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.5 Enterprise System Integrations, Vendor API)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound predictive-maintenance vendor event
//                  ("vendor.predictivemaintenance.prediction.v1") by running the vendor's raw
//                  feature inputs (equipment age, run hours since PM, historical failure count,
//                  sensor anomaly score) through the REAL, internal
//                  Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor -- the
//                  external vendor's own "shouldAlert"/"failureLikelihood" fields (if present in the
//                  payload) are deliberately ignored; only this platform's own deterministic model
//                  output decides whether to raise an alert, per Handbook §2.6 (Canonical Adapter
//                  Rule: "normalize external data ... never leak external error codes") and the
//                  originating task brief's explicit instruction not to let an external vendor's
//                  payload bypass internal validation/domain rules. When the recomputed likelihood
//                  crosses the model's own alert threshold, dispatches the platform's real, public
//                  Operational.PredictiveMaintenance.Commands.RaisePredictiveMaintenanceAlertCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Operational.PredictiveMaintenance.Commands;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Enterprise.Integrations.Vendor.Translators;

/// <summary>Translates an inbound predictive-maintenance vendor event, recomputed through the real internal predictor, into <see cref="RaisePredictiveMaintenanceAlertCommand"/> when warranted.</summary>
public sealed class VendorPredictiveMaintenanceInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "vendor.predictivemaintenance.prediction.v1";

    private static readonly FailureLikelihoodPredictor Predictor = new();

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Vendor &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("equipmentId", out var equipmentIdElement) || !equipmentIdElement.TryGetGuid(out var equipmentId))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
                ErrorType.Validation,
                "Vendor predictive-maintenance payload is missing a valid 'equipmentId'."));
        }

        var equipmentAgeMonths = payload.TryGetProperty("equipmentAgeMonths", out var ageElement) && ageElement.TryGetInt32(out var age) ? age : 0;
        var runHoursSinceLastPm = payload.TryGetProperty("runHoursSinceLastPm", out var runHoursElement) && runHoursElement.TryGetDouble(out var runHours) ? runHours : 0.0;
        var expectedPmIntervalHours = payload.TryGetProperty("expectedPmIntervalHours", out var intervalElement) && intervalElement.TryGetDouble(out var interval) && interval > 0
            ? interval
            : 720.0;
        var historicalFailureCount90Days = payload.TryGetProperty("historicalFailureCount90Days", out var failureCountElement) && failureCountElement.TryGetInt32(out var failureCount)
            ? failureCount
            : 0;
        var anomalyScore = payload.TryGetProperty("anomalyScore", out var anomalyElement) && anomalyElement.TryGetDouble(out var anomaly)
            ? anomaly
            : 0.0;

        var input = new FailureLikelihoodPredictionInput
        {
            EquipmentAgeMonths = equipmentAgeMonths,
            RunHoursSinceLastPm = runHoursSinceLastPm,
            ExpectedPmIntervalHours = expectedPmIntervalHours,
            HistoricalFailureCount90Days = historicalFailureCount90Days,
            AnomalyScore = anomalyScore,
        };

        var prediction = Predictor.Predict(input, UtcTimestamp.From(envelope.OccurredAtUtc));

        if (!prediction.Output.ShouldRaiseAlert)
            return Result<bool>.Success(true);

        var command = RaisePredictiveMaintenanceAlertCommand.FromDto(
            tenantId: envelope.TenantId,
            siteId: null,
            equipmentId: equipmentId,
            predictedFailureType: $"VendorPredictive:{Predictor.ModelName}:{prediction.Output.FailureLikelihood:F2}",
            predictedAtUtc: envelope.OccurredAtUtc,
            raisedByUserId: envelope.ActorId ?? Guid.Empty,
            correlationId: envelope.CorrelationId,
            causationId: envelope.CausationId ?? envelope.EventId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(result.Error!.ErrorCode)),
                ErrorType.Failure,
                result.Error.Message));
    }

    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
