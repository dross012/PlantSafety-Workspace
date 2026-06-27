// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertHistoryId.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      StrongId for AlertHistory entries. Required by the StrongId Enforcement
//      Rule (5.5) and Deterministic Identifier Rule (1.2). Backed by UUIDv7.
// ============================================================================

using System;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

public readonly record struct AlertHistoryId
{
    public Guid Value { get; }

    public AlertHistoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertHistoryId cannot be empty.");
        Value = value;
    }

    public static AlertHistoryId New() => new(Uuid7.New());
    public static AlertHistoryId FromGuid(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
