// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Vendor
//  Layer:          Application
//  File:           VendorRiskScoreInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.5 Enterprise System Integrations, Vendor API)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound external risk-scoring-system event
//                  ("vendor.riskscore.reported.v1") into a new risk assessment record referencing
//                  the external source: dispatches the platform's real, public
//                  RiskManagement.Commands.CreateRiskCommand (Phase 1 Step 1 -- hazard
//                  identification, with Location annotated to reference the external source per the
//                  originating task brief's guidance) followed by ScoreRiskCommand (Phase 1 Step 2 --
//                  records the external system's Severity x Likelihood score against the new
//                  record). This is the correct, real entry point: RiskManagement's CQRS surface has
//                  no dedicated "external risk import" command, so composing the two real, already-
//                  validated lifecycle commands is the grounded approach rather than fabricating a
//                  new one. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.RiskManagement.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Vendor.Translators;

/// <summary>Translates an inbound external risk-scoring-system event into a new, scored Risk record.</summary>
public sealed class VendorRiskScoreInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "vendor.riskscore.reported.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Vendor &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!TryGetString(payload, "riskTitle", out var riskTitle))
            return InvalidPayload("'riskTitle'");

        if (!TryGetString(payload, "hazardSource", out var hazardSource))
            return InvalidPayload("'hazardSource'");

        if (!payload.TryGetProperty("severity", out var severityElement) || !severityElement.TryGetInt32(out var severity))
            return InvalidPayload("'severity'");

        if (!payload.TryGetProperty("likelihood", out var likelihoodElement) || !likelihoodElement.TryGetInt32(out var likelihood))
            return InvalidPayload("'likelihood'");

        var externalSourceName = TryGetString(payload, "externalSourceName", out var sourceName) ? sourceName : "external risk-scoring system";
        var siteId = payload.TryGetProperty("siteId", out var siteElement) && siteElement.TryGetGuid(out var parsedSiteId)
            ? parsedSiteId
            : (Guid?)null;

        var createCommand = CreateRiskCommand.FromDto(
            tenantId: envelope.TenantId,
            siteId: siteId,
            riskTitle: riskTitle,
            location: $"Reported by {externalSourceName} (external event {envelope.EventId})",
            hazardSource: hazardSource,
            ownerId: envelope.ActorId ?? Guid.Empty,
            reviewRequired: true,
            correlationId: envelope.CorrelationId,
            causationId: envelope.CausationId ?? envelope.EventId);

        var createResult = await sender.Send(createCommand, cancellationToken);

        if (createResult.IsFailure)
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(createResult.Error.ErrorCode)),
                ErrorType.Failure,
                createResult.Error.Message));
        }

        var scoreCommand = new ScoreRiskCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            RequestedAtUtc: DateTime.UtcNow,
            TenantId: envelope.TenantId,
            RiskId: createResult.Value.RiskId,
            Severity: severity,
            Likelihood: likelihood,
            ScoredBy: envelope.ActorId ?? Guid.Empty);

        var scoreResult = await sender.Send(scoreCommand, cancellationToken);

        return scoreResult.IsSuccess
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error(
                ErrorCode.Create(NormalizeErrorCode(scoreResult.Error.ErrorCode)),
                ErrorType.Failure,
                scoreResult.Error.Message));
    }

    private static bool TryGetString(JsonElement payload, string propertyName, out string value)
    {
        value = string.Empty;
        if (!payload.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
            return false;

        var raw = element.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        value = raw;
        return true;
    }

    private static Result<bool> InvalidPayload(string missingField) =>
        Result<bool>.Failure(new Error(
            ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
            ErrorType.Validation,
            $"Vendor risk-score payload is missing a valid {missingField}."));

    private static string NormalizeErrorCode(string applicationErrorCode) =>
        applicationErrorCode.Contains('.') ? applicationErrorCode.ToLowerInvariant() : $"enterprise.eventmesh.{applicationErrorCode.ToLowerInvariant()}";
}
