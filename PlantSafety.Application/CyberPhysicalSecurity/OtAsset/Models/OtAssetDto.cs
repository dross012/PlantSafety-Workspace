// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTO for a single OtAsset inventory record.
// SchemaVersion: 1

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;

/// <summary>Read-model DTO for a single OT Asset Inventory record.</summary>
public sealed record OtAssetDto(
    Guid OtAssetId,
    Guid TenantId,
    Guid SiteId,
    string AssetTag,
    string AssetName,
    string AssetType,
    string Vendor,
    string Model,
    string FirmwareVersion,
    string NetworkZone,
    string Criticality,
    string Status,
    DateTime? LastReconciledAtUtc,
    DateTime CreatedAtUtc);

/// <summary>Result of a successful RegisterOtAssetCommand.</summary>
public sealed record RegisterOtAssetResult(Guid OtAssetId, string Status);
