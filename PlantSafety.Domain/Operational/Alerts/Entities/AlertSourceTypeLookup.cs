// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Lookups)
//  File:           AlertSourceTypeLookup.cs
//  Created:        2026-05-27
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Canonical lookup entity representing an Alert Source Type. Source Types
//      identify the origin of an alert (e.g., Sensor, Operator, Integration,
//      Rule Engine) and are used for routing, analytics, and filtering.
//
//  Architectural Notes:
//      • Lookup entities MUST be immutable.
//      • Source type codes MUST be strong-typed.
//      • Source types MUST remain stable for cross-module integrations.
//
//  Onboarding Notes:
//      • Populated from lookup tables.
//      • Used by validators, domain invariants, and read models.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertSourceTypeLookup(
    AlertSourceTypeCode SourceTypeCode,
    string DisplayName,
    string Description);
