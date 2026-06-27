// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Lookups)
//  File:           AlertCategoryLookup.cs
//  Created:        2026-05-27
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Canonical lookup entity representing an Alert Category. Categories group
//      alerts into high-level operational domains (e.g., Safety, Compliance,
//      Maintenance) and are used for dashboards, filtering, and analytics.
//
//  Architectural Notes:
//      • Lookup entities MUST be immutable.
//      • Category codes MUST be strong-typed.
//      • Categories MUST be stable for reporting.
//
//  Onboarding Notes:
//      • Populated from lookup tables.
//      • Used by validators, domain invariants, and read models.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertCategoryLookup(
    AlertCategoryCode CategoryCode,
    string DisplayName,
    string Description);
