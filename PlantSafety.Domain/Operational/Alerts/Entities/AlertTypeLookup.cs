// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Lookups)
//  File:           AlertTypeLookup.cs
//  Created:        2026-05-27
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Canonical lookup entity representing an Alert Type. Alert Types classify
//      alerts into functional categories (e.g., Hazard, Compliance, Equipment)
//      and are used across routing, analytics, dashboards, and domain logic.
//
//  Architectural Notes:
//      • Lookup entities MUST be immutable (Rule 3.1).
//      • Lookup entities MUST NOT contain domain behavior (Rule 1.1).
//      • Lookup entities MUST be strong-typed (Rule 3.4).
//      • Lookup entities MUST be stable for analytics and reporting.
//
//  Onboarding Notes:
//      • Populated from lookup tables via IAlertReadRepository.
//      • Mapped to AlertTypeLookupDto in the Application layer.
//      • Used by validators and domain invariants.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertTypeLookup(
    AlertTypeCode TypeCode,
    string DisplayName,
    string Description);
