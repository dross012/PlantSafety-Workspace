// ============================================================================
// File: ObservationQueryRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Internal Presentation-layer request models for Observation SafetyEvents queries and commands.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from validated Contracts DTOs. Passed to Application-layer constructors.
//
// Rationale:
// - Allows Presentation-layer enrichment (resolved TenantId, CallerUserId) before dispatching.
//
// Future Extension Seams:
// - Add CallerUserId or permission context here when commands require Presentation-layer enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.Observation.Models.Requests;

/// <summary>Internal Presentation-layer command request for Observation SafetyEvents create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="Title">Record title.</param>
/// <param name="Description">Detailed description.</param>
/// <param name="ScheduledDate">Scheduled date (UTC).</param>
/// <param name="AssignedToUserId">Optional assignee.</param>
public sealed record ObservationCommandRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   Title,
    string?  Description,
    DateTime ScheduledDate,
    Guid?    AssignedToUserId);