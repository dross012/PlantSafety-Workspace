// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Strong Types)
//  File:           AlertSeverityCode.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed canonical code representing an Alert Severity. Severity
//      codes classify alerts into operational priority levels and are used
//      throughout the lifecycle, routing, and escalation logic.
//
//  Architectural Notes:
//      • Strong types MUST be immutable (Rule 3.1).
//      • Severity codes MUST be validated in the Domain layer (Rule 3.4).
//      • Strong types MUST NOT allow empty values (Rule 3.2).
//      • Severity codes MUST be stable for analytics and reporting.
//
//  Onboarding Notes:
//      • Used by Commands, Domain logic, Validators, and Read Models.
//      • Ensures severity values are never passed as raw strings.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

public readonly record struct AlertSeverityCode(string Value)
{
    public override string ToString() => Value;
}
