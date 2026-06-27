// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Lookups)
//  File:           AlertSeverityLookup.cs
//  Created:        2026-05-27
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Canonical lookup entity representing an Alert Severity. Severity levels
//      define operational urgency and drive escalation, routing, and SLA logic.
//
//  Architectural Notes:
//      • Lookup entities MUST be immutable.
//      • Severity codes MUST be strong-typed.
//      • Severity definitions MUST remain stable for analytics.
//
//  Onboarding Notes:
//      • Populated from lookup tables.
//      • Used by validators, domain invariants, and read models.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertSeverityLookup(
    AlertSeverityCode SeverityCode,
    string DisplayName,
    string Description);
