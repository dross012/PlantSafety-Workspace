// ============================================================================
//  File:           DeviceId.cs
//  Created:        2026-05-26
// ============================================================================

namespace PlantSafety.SharedKernel.Identifiers;

public sealed record DeviceId
{
    public string Value { get; }

    public DeviceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DeviceId cannot be empty.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
