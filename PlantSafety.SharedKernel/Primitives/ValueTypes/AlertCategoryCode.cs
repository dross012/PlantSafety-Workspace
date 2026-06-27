// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Strong Types)
//  File:           AlertCategoryCode.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed canonical code representing an Alert Category. This type
//      prevents accidental mixing of category codes with other lookup codes.
//
//  Architectural Notes:
//      • Strong types MUST be immutable (Rule 3.1).
//      • Strong types MUST NOT allow empty values (Rule 3.2).
//      • Strong types MUST be comparable and serializable.
//
//  Onboarding Notes:
//      • Used across Commands, Domain, and Read Models.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

public readonly record struct AlertCategoryCode(string Value)
{
    public override string ToString() => Value;
}
