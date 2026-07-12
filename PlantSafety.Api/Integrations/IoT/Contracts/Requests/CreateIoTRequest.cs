// ============================================================================
// File: CreateIoTRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new Integrations.IoT record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateIoTCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add file attachment metadata here when Integrations.IoT records support document uploads.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.IoT.Contracts.Requests;

/// <summary>Request body for creating a new Integrations.IoT record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="DeviceLabel">Human-readable label of the originating IoT device.</param>
/// <param name="Description">Detailed description or notes.</param>
/// <param name="ScheduledAtUtc">Date/time the IoT activity is scheduled (UTC).</param>
/// <param name="AssignedToUserId">Optional assigned user.</param>
public sealed record CreateIoTRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   DeviceLabel,
    string?  Description,
    DateTime ScheduledAtUtc,
    Guid?    AssignedToUserId);
