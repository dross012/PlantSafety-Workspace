// ============================================================================
// File: CreateProcessRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for POST /features/process-management/processes,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Requests;

/// <summary>Request body for creating a new Process Definition.</summary>
/// <param name="SiteId">Site this process is scoped to. Null means enterprise-level.</param>
/// <param name="ProcessName">Human-readable process name.</param>
/// <param name="Category">Safety-critical process category.</param>
/// <param name="OwnerId">Identity of the accountable process owner.</param>
public sealed record CreateProcessRequest(
    Guid?  SiteId,
    string ProcessName,
    string Category,
    Guid   OwnerId);
