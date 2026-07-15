// ============================================================================
// File: RealtimeGroups.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform
// Created: 2026-07-15
// Description: Canonical SignalR group-name constants for PlantSafetyRealtimeHub.
//   Group names are tenant-scoped ("{module}:{tenantId}") so a broadcast for one
//   tenant is never delivered to another tenant's connections (multi-tenant isolation
//   — see Enterprise Arch Handbook Part XVII, Identity/Tenancy rules).
// ============================================================================

namespace PlantSafety.Api.Realtime;

/// <summary>Canonical SignalR group-name builders for the shared PlantSafety real-time hub.</summary>
public static class RealtimeGroups
{
    /// <summary>Group name for Production Tracking Dashboard subscribers for a given tenant (Volume T1).</summary>
    public static string ProductionTracking(Guid tenantId) => $"production-tracking:{tenantId}";

    /// <summary>Group name for Incident SLA Monitor subscribers for a given tenant (Volume ISM1).</summary>
    public static string IncidentSla(Guid tenantId) => $"incident-sla:{tenantId}";
}
