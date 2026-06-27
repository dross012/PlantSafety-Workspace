// ============================================================================
// File: CreateWorkInstructionsRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: External POST request DTO for creating a new WorkInstructions compliance record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateWorkInstructionsCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add file attachment metadata here when WorkInstructions records support document uploads.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.WorkInstructions.Contracts.Requests;

/// <summary>Request body for creating a new WorkInstructions compliance record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="Title">Short descriptive title.</param>
/// <param name="Description">Detailed description or notes.</param>
/// <param name="ScheduledDate">Date the WorkInstructions activity is scheduled (UTC).</param>
/// <param name="AssignedToUserId">Optional assigned user.</param>
public sealed record CreateWorkInstructionsRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   Title,
    string?  Description,
    DateTime ScheduledDate,
    Guid?    AssignedToUserId);