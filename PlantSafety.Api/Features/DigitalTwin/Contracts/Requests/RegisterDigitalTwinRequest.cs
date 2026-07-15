// ============================================================================
// File: RegisterDigitalTwinRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for POST v1/digital-twin/twins,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.DigitalTwin.Contracts.Requests;

/// <summary>Request body for registering a new Digital Twin.</summary>
/// <param name="SiteId">Site this twin is scoped to. Null means tenant-wide.</param>
/// <param name="AssetName">Display name of the physical asset this twin represents.</param>
/// <param name="AssetType">Asset type/category (e.g., Machine, Line, Facility).</param>
public sealed record RegisterDigitalTwinRequest(
    Guid?  SiteId,
    string AssetName,
    string AssetType);
