// ============================================================================
// File: ImpersonationRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// Created: 2026-08-03
// Description: External-facing request DTOs for the Support.Impersonation module's V1 controller.
// ============================================================================

namespace PlantSafety.Api.Support.Impersonation.Contracts.Requests;

/// <summary>Request body for starting a tenant-impersonation session.</summary>
public sealed record StartImpersonationRequest
{
    /// <summary>Mandatory, human-readable business reason for the impersonation (recorded in the audit trail).</summary>
    public string Reason { get; init; } = string.Empty;
}
