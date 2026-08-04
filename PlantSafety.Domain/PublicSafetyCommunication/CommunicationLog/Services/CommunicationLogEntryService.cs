// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for CommunicationLogEntry. Coordinates logic that cannot live on
//                  a single aggregate (e.g., assembling the After-Action Communication Timeline
//                  across multiple entries for an incident).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Services;

/// <summary>Domain service for CommunicationLogEntry. Coordinates logic that cannot live on a single aggregate.</summary>
public class CommunicationLogEntryService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
