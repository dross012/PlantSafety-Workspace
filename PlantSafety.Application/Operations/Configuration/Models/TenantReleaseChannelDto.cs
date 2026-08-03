// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           TenantReleaseChannelDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (§7.2 Release Pipeline) read/write model for a tenant's assigned
//                  TenantReleaseChannel. Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>A tenant's assigned release channel, plus audit metadata.</summary>
/// <param name="TenantId">The tenant this channel assignment applies to.</param>
/// <param name="Channel">The assigned release channel.</param>
/// <param name="UpdatedUtc">When the channel was last changed.</param>
/// <param name="UpdatedBy">Who last changed the channel.</param>
public sealed record TenantReleaseChannelDto(
    Guid TenantId,
    TenantReleaseChannel Channel,
    DateTime UpdatedUtc,
    string UpdatedBy);
