// ============================================================================
// File: DigitalTwinAssetResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTO for a single Digital Twin asset record.
// ============================================================================

namespace PlantSafety.Api.Features.DigitalTwin.Contracts.Responses;

/// <summary>Response payload for a single Digital Twin asset record.</summary>
public sealed record DigitalTwinAssetResponse(
    Guid     TwinId,
    Guid     TenantId,
    Guid?    SiteId,
    string   AssetName,
    string   AssetType,
    string   Status,
    DateTime LastSyncAtUtc,
    double   TelemetryHealthScore);

/// <summary>Envelope response payload for the Digital Twin asset registry listing.</summary>
public sealed record DigitalTwinListResponse(
    Guid                                      TenantId,
    Guid?                                     SiteId,
    DateTime                                  GeneratedAt,
    IReadOnlyList<DigitalTwinAssetResponse>   Items);

/// <summary>Response payload returned after registering a Digital Twin.</summary>
/// <param name="TwinId">Identifier of the newly registered twin.</param>
public sealed record RegisterDigitalTwinResponse(Guid TwinId);
