// ============================================================================
// File: ProcessResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTO for a single Process Definition record.
// ============================================================================

namespace PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Responses;

/// <summary>Response payload for a single Process Definition record.</summary>
public sealed record ProcessResponse(
    Guid           ProcessId,
    Guid           TenantId,
    Guid?          SiteId,
    string         ProcessName,
    string         Category,
    string         Status,
    int            Version,
    Guid           OwnerId,
    DateTimeOffset LastModifiedAtUtc,
    string         ETag);
