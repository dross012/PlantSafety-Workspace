// ============================================================================
//  File:           CorrelationId.cs
//  Created:        2026-05-26
// ============================================================================

using System;

namespace PlantSafety.SharedKernel.Identifiers;

public readonly record struct CorrelationId
{
    public Guid Value { get; }

    public CorrelationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CorrelationId cannot be empty.");
        Value = value;
    }

    public static CorrelationId New() => new(Uuid7.New());
    public override string ToString() => Value.ToString("D");
}
