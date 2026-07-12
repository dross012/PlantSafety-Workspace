// ============================================================================
// File: CreateInternalRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new Internal record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateInternalCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add additional metadata fields here as the Internal record schema evolves.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Requests;

/// <summary>Request body for creating a new Internal record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="RecipientUserId">User the internal notification was delivered to.</param>
/// <param name="Message">Notification message body.</param>
/// <param name="Category">Notification category (e.g., Alert/Reminder/Info).</param>
/// <param name="TriggeredBy">Identity of the user or process that triggered the notification.</param>
public sealed record CreateInternalRequest(
    Guid     TenantId,
    Guid     SiteId,
    Guid   RecipientUserId,
    string   Message,
    string?   Category,
    Guid?    TriggeredBy);