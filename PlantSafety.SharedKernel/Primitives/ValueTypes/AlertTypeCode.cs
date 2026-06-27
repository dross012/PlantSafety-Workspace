// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Strong Types)
//  File:           AlertTypeCode.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed canonical code representing an Alert Type.
//
//  Architectural Notes:
//      • Strong types MUST be immutable.
//      • Type codes MUST be validated in the Domain layer.
//
//  Onboarding Notes:
//      • Used by lifecycle transitions and read models.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

public readonly record struct AlertTypeCode(string Value)
{
    public override string ToString() => Value;
}
