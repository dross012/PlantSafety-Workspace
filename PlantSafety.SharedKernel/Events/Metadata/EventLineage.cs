/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventLineage.cs
    DESCRIPTION:        Strong-typed lineage ValueObject containing correlation and causation identifiers. Enforces the
                        Canonical Correlation & Causation Rule and deterministic propagation.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Strong-typed lineage information for domain events.
/// </summary>
public sealed record EventLineage
{
    public string CorrelationId { get; }
    public string? CausationId { get; }

    private EventLineage(string correlationId, string? causationId)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
    }

    public static EventLineage Create(string correlationId, string? causationId)
    {
        correlationId = correlationId?.Trim() ?? throw new ArgumentNullException(nameof(correlationId));

        if (correlationId.Length == 0)
            throw new ArgumentException("CorrelationId cannot be empty.");

        if (causationId != null)
        {
            causationId = causationId.Trim();
            if (causationId.Length == 0)
                causationId = null;
        }

        return new EventLineage(correlationId, causationId);
    }
}
