// ============================================================================
//  File:           AlertOutboxEventId.cs
//  Created:        2026-05-26
// ============================================================================

using System;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

public readonly record struct AlertOutboxEventId
{
    public Guid Value { get; }

    public AlertOutboxEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertOutboxEventId cannot be empty.");
        Value = value;
    }

    public static AlertOutboxEventId New() => new(Uuid7.New());
    public static AlertOutboxEventId FromGuid(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
