// ============================================================================
// File: SetTenantReleaseChannelRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Request body for PUT /api/v1/operations/configuration/tenants/{tenantId}/release-channel.
// ============================================================================
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Api.Operations.Configuration.Contracts.Requests;

/// <summary>Request body to assign a tenant's release channel.</summary>
/// <param name="Channel">The channel to assign (Stable, Beta, or Canary).</param>
public sealed record SetTenantReleaseChannelRequest(TenantReleaseChannel Channel);
