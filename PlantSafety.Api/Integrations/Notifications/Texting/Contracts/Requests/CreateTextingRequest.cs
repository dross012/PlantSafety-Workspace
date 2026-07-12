// ============================================================================
// File: CreateTextingRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new Texting record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateTextingCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add additional metadata fields here as the Texting record schema evolves.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Texting.Contracts.Requests;

/// <summary>Request body for creating a new Texting record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="RecipientPhoneNumber">Phone number the notification SMS was delivered to.</param>
/// <param name="MessageBody">Text content of the notification SMS.</param>
/// <param name="TriggerEvent">Name of the platform event that triggered the SMS notification.</param>
/// <param name="SentBy">Identity of the user or process that triggered the notification.</param>
public sealed record CreateTextingRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   RecipientPhoneNumber,
    string   MessageBody,
    string?   TriggerEvent,
    Guid?    SentBy);