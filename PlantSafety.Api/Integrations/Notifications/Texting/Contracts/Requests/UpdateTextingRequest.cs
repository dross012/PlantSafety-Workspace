// ============================================================================
// File: UpdateTextingRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing Texting record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateTextingCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Texting.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Texting.Contracts.Requests;

/// <summary>Request body for updating an existing Texting record.</summary>
/// <param name="RecipientPhoneNumber">Updated phone number the notification sms was delivered to.</param>
/// <param name="MessageBody">Updated text content of the notification sms.</param>
/// <param name="TriggerEvent">Updated name of the platform event that triggered the sms notification.</param>
/// <param name="SentBy">Updated identity of the user or process that triggered the notification.</param>
/// <param name="Status">Updated status value.</param>
public sealed record UpdateTextingRequest(
    string   RecipientPhoneNumber,
    string   MessageBody,
    string?   TriggerEvent,
    Guid?    SentBy,
    string   Status);