// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Strong Types)
//  File:           AlertSourceTypeCode.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed canonical code representing an Alert Source Type. Source
//      types identify the origin of an alert (e.g., sensor, operator, system,
//      integration) and are used for routing, analytics, and filtering.
//
//  Architectural Notes:
//      • Strong types MUST be immutable (Rule 3.1).
//      • Source type codes MUST be validated in the Domain layer (Rule 3.4).
//      • Strong types MUST NOT allow empty values (Rule 3.2).
//      • Source types MUST remain stable for cross-module integrations.
//
//  Onboarding Notes:
//      • Used across Commands, Domain logic, Validators, and Read Models.
//      • Prevents mixing source type codes with other lookup categories.
// ============================================================================

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

public readonly record struct AlertSourceTypeCode(string Value)
{
    public override string ToString() => Value;
}

