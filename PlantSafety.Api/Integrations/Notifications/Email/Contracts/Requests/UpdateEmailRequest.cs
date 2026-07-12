// ============================================================================
// File: UpdateEmailRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing Email record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateEmailCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Email.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Email.Contracts.Requests;

/// <summary>Request body for updating an existing Email record.</summary>
/// <param name="RecipientAddress">Updated email address the notification was delivered to.</param>
/// <param name="Subject">Updated subject line of the notification email.</param>
/// <param name="TemplateName">Updated name of the email template used to render the notification.</param>
/// <param name="SentBy">Updated identity of the user or process that triggered the notification.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateEmailRequest(
    string   RecipientAddress,
    string   Subject,
    string?   TemplateName,
    Guid?    SentBy,
    string   Status);