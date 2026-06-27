// ============================================================================
// File: UpdateBackupRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External PUT body DTO for updating a Backup CrossDomain record.
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Backup.Contracts.Requests;

/// <summary>Payload for updating an existing Backup CrossDomain record.</summary>
/// <param name="Name">Updated job name.</param>
/// <param name="Description">Updated description.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateBackupRequest(
    string  Name,
    string? Description,
    string  Status);
