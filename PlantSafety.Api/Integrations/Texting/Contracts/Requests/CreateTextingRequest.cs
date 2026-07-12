// ============================================================================
// File: CreateTextingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new Integrations.Texting record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateTextingCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add file attachment metadata here when Integrations.Texting records support document uploads.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Texting.Contracts.Requests;

/// <summary>Request body for creating a new Integrations.Texting record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="RecipientPhoneNumber">Destination phone number in E.164 format.</param>
/// <param name="Description">Detailed description or notes.</param>
/// <param name="ScheduledAtUtc">Date/time the Texting activity is scheduled (UTC).</param>
/// <param name="AssignedToUserId">Optional assigned user.</param>
public sealed record CreateTextingRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   RecipientPhoneNumber,
    string?  Description,
    DateTime ScheduledAtUtc,
    Guid?    AssignedToUserId);
