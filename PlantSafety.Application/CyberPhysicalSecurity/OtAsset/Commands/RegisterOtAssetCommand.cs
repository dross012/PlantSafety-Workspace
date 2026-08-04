// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to register a new OtAsset in the OT Asset Inventory.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Commands;

/// <summary>Registers a new OtAsset (OT Asset Inventory subdomain).</summary>
public sealed record RegisterOtAssetCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
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
    Guid ActorId) : IRequest<Result<RegisterOtAssetResult>>;
