// ============================================================================
// File: CreateBackupRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External POST body DTO for initiating a Backup CrossDomain operation.
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Backup.Contracts.Requests;

/// <summary>Payload for initiating a new Backup CrossDomain operation.</summary>
/// <param name="TenantId">Owning tenant identifier.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="Name">Backup job name.</param>
/// <param name="Description">Optional description.</param>
public sealed record CreateBackupRequest(
    Guid    TenantId,
    Guid?   SiteId,
    string  Name,
    string? Description);
