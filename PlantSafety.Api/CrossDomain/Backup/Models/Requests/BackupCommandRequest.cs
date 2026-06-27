// ============================================================================
// File: BackupCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Internal Presentation-layer command request model for Backup CrossDomain.
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Backup.Models.Requests;

/// <summary>Internal Presentation-layer command request model for Backup CrossDomain operations.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="Name">Backup job name.</param>
/// <param name="Description">Optional description.</param>
/// <param name="Status">Current status.</param>
public sealed record BackupCommandRequest(
    Guid    TenantId,
    Guid?   SiteId,
    string  Name,
    string? Description,
    string? Status);
