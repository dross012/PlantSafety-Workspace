// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Strong Types)
//  File:           AlertStatusCode.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed canonical code representing an Alert Status.
//
//  Architectural Notes:
//      • Strong types MUST be immutable.
//      • Status codes MUST be validated in the Domain layer.
//
//  Onboarding Notes:
//      • Used by lifecycle transitions and read models.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

public readonly record struct AlertStatusCode(string Value)
{
    public override string ToString() => Value;
}
