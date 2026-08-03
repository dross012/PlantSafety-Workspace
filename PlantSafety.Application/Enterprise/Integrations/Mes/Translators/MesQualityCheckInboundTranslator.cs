// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Mes
//  Layer:          Application
//  File:           MesQualityCheckInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.2 Enterprise System Integrations, MES)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound MES quality-check event ("mes.qualitycheck.recorded.v1")
//                  into the platform's real, public
//                  Operational.ProductionTracking.Commands.RecordProductionRunEventCommand
//                  (RunEventType="QualityCheck") -- Operational.ProductionTracking (checked first, per
//                  the originating task brief) is the correct, real entry point; IndustrialHygiene's
//                  command surface is an empty scaffold with no real quality-check command. Additive-
//                  only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Operational.ProductionTracking.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Mes.Translators;

/// <summary>Translates an inbound MES quality-check event into <see cref="RecordProductionRunEventCommand"/>.</summary>
public sealed class MesQualityCheckInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "mes.qualitycheck.recorded.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Mes &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!payload.TryGetProperty("productionRunId", out var runIdElement) || !runIdElement.TryGetGuid(out var runId))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
                ErrorType.Validation,
                "MES quality-check payload is missing a valid 'productionRunId'."));
        }

        var passedCount = payload.TryGetProperty("passedCount", out var passedElement) && passedElement.TryGetInt32(out var passed) ? passed : 0;
        var failedCount = payload.TryGetProperty("failedCount", out var failedElement) && failedElement.TryGetInt32(out var failed) ? failed : 0;

        var checkDescription = payload.TryGetProperty("checkDescription", out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String
            ? descriptionElement.GetString() ?? "External MES quality check."
            : "External MES quality check.";

        var command = new RecordProductionRunEventCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            TenantId: envelope.TenantId,
            RunId: runId,
            RunEventType: "QualityCheck",
            Description: checkDescription,
            IsDowntime: false,
            DowntimeReasonCode: null,
            DowntimeMinutes: 0,
            GoodUnitsDelta: passedCount,
            ScrapUnitsDelta: failedCount,
            RequestedByUserId: envelope.ActorId ?? Guid.Empty);

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
