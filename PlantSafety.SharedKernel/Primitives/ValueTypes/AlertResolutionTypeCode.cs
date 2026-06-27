// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Strong Types)
//  File:           AlertResolutionTypeCode.cs
//  Created:        2026-05-27
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed canonical code representing an Alert Resolution Type.
//      Resolution types classify how an alert was resolved (e.g., Fixed,
//      Dismissed, Deferred, No Action Required) and are used across reporting,
//      analytics, and compliance workflows.
//
//  Architectural Notes:
//      • Strong types MUST be immutable (Rule 3.1).
//      • Resolution type codes MUST be validated in the Domain layer (Rule 3.4).
//      • Strong types MUST NOT allow empty or null values (Rule 3.2).
//      • Resolution types MUST remain stable for compliance reporting.
//
//  Onboarding Notes:
//      • Used across Commands, Domain logic, Validators, and Read Models.
//      • Prevents mixing resolution type codes with other lookup categories.
//      • Ensures type safety when interacting with lookup tables.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

public readonly record struct AlertResolutionTypeCode(string Value)
{
    public override string ToString() => Value;
}
