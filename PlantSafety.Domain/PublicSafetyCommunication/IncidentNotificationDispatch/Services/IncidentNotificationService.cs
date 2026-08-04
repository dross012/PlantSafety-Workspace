// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for IncidentNotification. Coordinates logic that cannot live on
//                  a single aggregate (e.g., evaluating escalation across multiple outstanding
//                  notifications for an incident).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Services;

/// <summary>Domain service for IncidentNotification. Coordinates logic that cannot live on a single aggregate.</summary>
public class IncidentNotificationService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
