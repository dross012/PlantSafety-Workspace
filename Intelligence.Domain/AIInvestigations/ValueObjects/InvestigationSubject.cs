/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationSubject.cs
    DESCRIPTION:        Value Object for the subject description of an AI investigation. Normalized at
                        creation: trimmed, CR/LF stripped. Max length from IntelligenceConstants. (Rule 2.8)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.AIInvestigations.ValueObjects;

/// <summary>The subject of an AI investigation. Normalized and length-bounded.</summary>
public sealed class InvestigationSubject : ValueObject
{
    /// <summary>The normalized subject string.</summary>
    public string Value { get; }

    private InvestigationSubject(string value) => Value = value;

    /// <summary>Creates an <see cref="InvestigationSubject"/> from a raw string.</summary>
    /// <exception cref="ArgumentException">Thrown when empty or exceeds max length.</exception>
    public static InvestigationSubject From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("InvestigationSubject cannot be empty.", nameof(value));

        var normalized = value.Trim()
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal);

        if (normalized.Length > IntelligenceConstants.MaxInvestigationSubjectLength)
            throw new ArgumentException(
                $"InvestigationSubject must not exceed {IntelligenceConstants.MaxInvestigationSubjectLength} characters. Received: {normalized.Length}.",
                nameof(value));

        return new InvestigationSubject(normalized);
    }

    /// <summary>Implicit conversion to string.</summary>
    public static implicit operator string(InvestigationSubject s) => s.Value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
