// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Lookups)
//  File:           AlertResolutionTypeLookup.cs
//  Created:        2026-05-27
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Canonical lookup entity representing an Alert Resolution Type. These
//      values classify how an alert was resolved (e.g., Fixed, Dismissed,
//      Deferred, No Action Required) and are used for reporting and analytics.
//
//  Architectural Notes:
//      • Lookup entities MUST be immutable.
//      • Resolution type codes MUST be strong-typed.
//      • Resolution types MUST remain stable for compliance reporting.
//
//  Onboarding Notes:
//      • Populated from lookup tables.
//      • Used by validators, domain invariants, and read models.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertResolutionTypeLookup(
    AlertResolutionTypeCode ResolutionTypeCode,
    string DisplayName,
    string Description);
