// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           ITenantReleaseChannelRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real [Release].[TenantReleaseChannel]
//                  table (Phase 7 §7.2, migration
//                  200-tenant-user-region-feature-flag-overrides.sql, PlantSafety.ArchiveDB).
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Repositories;

/// <summary>Repository over the real [Release].[TenantReleaseChannel] table.</summary>
public interface ITenantReleaseChannelRepository
{
    /// <summary>
    /// Returns the tenant's assigned release channel, or null if the tenant has never been
    /// assigned one (callers should treat a missing row as <see cref="TenantReleaseChannel.Stable"/>).
    /// </summary>
    Task<TenantReleaseChannelDto?> FindAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Creates or updates (upserts) the tenant's assigned release channel.</summary>
    /// <param name="tenantId">The tenant to assign a channel to.</param>
    /// <param name="channel">The channel to assign.</param>
    /// <param name="updatedBy">The identity of the caller making the change.</param>
    Task<TenantReleaseChannelDto> SetAsync(Guid tenantId, TenantReleaseChannel channel, string updatedBy, CancellationToken cancellationToken);
}
