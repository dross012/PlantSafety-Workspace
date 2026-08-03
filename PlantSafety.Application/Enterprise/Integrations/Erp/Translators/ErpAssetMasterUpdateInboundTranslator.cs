// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Application
//  File:           ErpAssetMasterUpdateInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound ERP asset-master-update event ("erp.asset.masterupdate.v1")
//                  into a real, normalized upsert against the Machines reference-data cache via
//                  IMachineAssetReferenceDataIngestor -- CrossDomain.Machines is a projection-only
//                  bounded context (Handbook Part XXII) with no CQRS Command surface, so this is an
//                  ETL-style cache ingestion, not a fabricated "command" (see the ingestor
//                  interface's own header for the full rationale). Registered as one of possibly
//                  many IEnterpriseInboundEventTranslator implementations resolved by DI. Additive-
//                  only evolution.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.CrossDomain.Machines.Ingestion;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Erp.Translators;

/// <summary>Translates an inbound ERP asset-master-update event into a Machines reference-data cache upsert.</summary>
public sealed class ErpAssetMasterUpdateInboundTranslator(IMachineAssetReferenceDataIngestor ingestor) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "erp.asset.masterupdate.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Erp &&
        string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = envelope.Payload;

        if (!TryGetGuid(payload, "assetId", out var assetId))
            return InvalidPayload("'assetId'");

        if (!TryGetString(payload, "assetName", out var assetName))
            return InvalidPayload("'assetName'");

        if (!TryGetString(payload, "assetType", out var assetType))
            return InvalidPayload("'assetType'");

        if (!TryGetString(payload, "status", out var status))
            return InvalidPayload("'status'");

        var serialNumber = payload.TryGetProperty("serialNumber", out var serialElement) && serialElement.ValueKind == JsonValueKind.String
            ? serialElement.GetString() ?? string.Empty
            : string.Empty;

        var lastServicedAtUtc = payload.TryGetProperty("lastServicedAtUtc", out var lastServicedElement) &&
            lastServicedElement.ValueKind == JsonValueKind.String && lastServicedElement.TryGetDateTime(out var parsedLastServiced)
            ? parsedLastServiced
            : envelope.OccurredAtUtc;

        var lastServicedBy = payload.TryGetProperty("lastServicedBy", out var lastServicedByElement) &&
            lastServicedByElement.TryGetGuid(out var parsedLastServicedBy)
            ? parsedLastServicedBy
            : envelope.ActorId ?? Guid.Empty;

        var siteId = payload.TryGetProperty("siteId", out var siteElement) && siteElement.TryGetGuid(out var parsedSiteId)
            ? parsedSiteId
            : (Guid?)null;

        var update = new ExternalMachineAssetUpdate(
            MachineId: assetId,
            TenantId: envelope.TenantId,
            SiteId: siteId,
            MachineName: assetName,
            MachineType: assetType,
            SerialNumber: serialNumber,
            Status: status,
            LastServicedAtUtc: lastServicedAtUtc,
            LastServicedBy: lastServicedBy);

        await ingestor.UpsertAsync(update, cancellationToken);

        return Result<bool>.Success(true);
    }

    private static bool TryGetGuid(JsonElement payload, string propertyName, out Guid value)
    {
        value = Guid.Empty;
        return payload.TryGetProperty(propertyName, out var element) && element.TryGetGuid(out value);
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
            $"ERP asset-master-update payload is missing a valid {missingField}."));
}
