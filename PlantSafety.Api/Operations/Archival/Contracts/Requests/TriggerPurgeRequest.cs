// ============================================================================
// File: TriggerPurgeRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: Request body for POST /api/v1/operations/archival/purge/trigger.
// ============================================================================
using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Api.Operations.Archival.Contracts.Requests;

/// <summary>Request body to trigger an on-demand retention purge.</summary>
/// <param name="Target">The single target to purge, or null to purge all 5 targets.</param>
public sealed record TriggerPurgeRequest(PurgeTarget? Target);
