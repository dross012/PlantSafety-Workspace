// ============================================================================
// File: CreateEmailRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new email delivery record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateEmailCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add message body/attachment metadata here when Email records support full content archival.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Email.Contracts.Requests;

/// <summary>Request body for creating a new email delivery record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="Recipient">Recipient email address.</param>
/// <param name="Subject">Subject line of the email.</param>
/// <param name="Provider">External email gateway/provider used for delivery (e.g., SendGrid, SMTP relay).</param>
public sealed record CreateEmailRequest(
    Guid   TenantId,
    Guid   SiteId,
    string Recipient,
    string Subject,
    string Provider);
