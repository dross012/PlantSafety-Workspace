// ============================================================================
// File: CreateEmailRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new Email record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateEmailCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add additional metadata fields here as the Email record schema evolves.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Email.Contracts.Requests;

/// <summary>Request body for creating a new Email record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="RecipientAddress">Email address the notification was delivered to.</param>
/// <param name="Subject">Subject line of the notification email.</param>
/// <param name="TemplateName">Name of the email template used to render the notification.</param>
/// <param name="SentBy">Identity of the user or process that triggered the notification.</param>
public sealed record CreateEmailRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   RecipientAddress,
    string   Subject,
    string?   TemplateName,
    Guid?    SentBy);