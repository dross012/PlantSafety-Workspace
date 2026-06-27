// ============================================================================
//  File:           CausationId.cs
//  Created:        2026-05-26
// ============================================================================

using System;

namespace PlantSafety.SharedKernel.Identifiers;

public readonly record struct CausationId
{
    public Guid Value { get; }

    public CausationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CausationId cannot be empty.");
        Value = value;
    }

    public static CausationId New() => new(Uuid7.New());
    public override string ToString() => Value.ToString("D");
}
