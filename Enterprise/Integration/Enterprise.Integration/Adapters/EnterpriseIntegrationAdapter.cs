// ============================================================================
//  Project:        Enterprise.Integration
//  Module:         Adapters
//  File:           EnterpriseIntegrationAdapter.cs
//  Created:        2026-08-08
//  Description:    Phase D.9: deterministically adapts a raw external
//                  system payload into an EnterpriseIntegrationEvent, by
//                  applying an explicit EnterpriseIntegrationMapping list.
//                  Fields the mapping doesn't cover are dropped, never
//                  guessed or passed through verbatim -- an unmapped field
//                  reaching the platform's internal model would be exactly
//                  the kind of implicit, undocumented coupling this
//                  explicit-mapping approach exists to avoid.
// ============================================================================
using Enterprise.Integration.Events;
using Enterprise.Integration.Mapping;

namespace Enterprise.Integration.Adapters;

/// <summary>Deterministically adapts a raw external system payload into an <see cref="EnterpriseIntegrationEvent"/>.</summary>
public interface IEnterpriseIntegrationAdapter
{
    /// <summary>The external system this adapter maps events from.</summary>
    EnterpriseIntegrationSystem SourceSystem { get; }

    /// <summary>Adapts a raw payload into a mapped <see cref="EnterpriseIntegrationEvent"/>.</summary>
    EnterpriseIntegrationEvent Adapt(string eventType, IReadOnlyDictionary<string, string> rawPayload, DateTime occurredAtUtc);
}

/// <summary><see cref="IEnterpriseIntegrationAdapter"/> implementation driven by an explicit <see cref="EnterpriseIntegrationMapping"/> list.</summary>
public sealed class EnterpriseIntegrationAdapter(EnterpriseIntegrationSystem sourceSystem, IReadOnlyList<EnterpriseIntegrationMapping> mappings) : IEnterpriseIntegrationAdapter
{
    /// <inheritdoc />
    public EnterpriseIntegrationSystem SourceSystem { get; } = sourceSystem;

    /// <inheritdoc />
    /// <exception cref="ArgumentException"><paramref name="eventType"/> is null or whitespace.</exception>
    public EnterpriseIntegrationEvent Adapt(string eventType, IReadOnlyDictionary<string, string> rawPayload, DateTime occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("eventType is required.", nameof(eventType));
        }

        var mappedPayload = new Dictionary<string, string>();
        foreach (EnterpriseIntegrationMapping mapping in mappings)
        {
            if (rawPayload.TryGetValue(mapping.ExternalFieldName, out string? value))
            {
                mappedPayload[mapping.InternalFieldName] = value;
            }
        }

        return new EnterpriseIntegrationEvent(Guid.NewGuid(), SourceSystem, eventType, mappedPayload, occurredAtUtc);
    }
}
