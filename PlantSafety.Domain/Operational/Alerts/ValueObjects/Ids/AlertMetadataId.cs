// ============================================================================
//  File:           AlertMetadataId.cs
//  Created:        2026-05-26
// ============================================================================

using System;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

public readonly record struct AlertMetadataId
{
    public Guid Value { get; }

    public AlertMetadataId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertMetadataId cannot be empty.");
        Value = value;
    }

    public static AlertMetadataId New() => new(Uuid7.New());
    public static AlertMetadataId FromGuid(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
