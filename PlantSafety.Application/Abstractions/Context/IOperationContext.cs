// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Operation context interface carrying the full correlation lineage envelope.
//   Every layer MUST propagate this envelope (§2.17-2.18 Correlation & Causality Lineage Rule).
//   CorrelationId is generated once at the API boundary and remains constant for the entire operation.
//   CausationId identifies the immediate parent command/event and is NEVER regenerated for child operations.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Context;

/// <summary>
/// Carries the full canonical correlation lineage envelope required by §2.17-2.18.
/// This context is injected into every handler, propagated through all layers,
/// and included in every structured log entry and domain event.
/// </summary>
/// <remarks>
/// Causation flow: Root commands generate new <c>CausationId</c> → child commands inherit
/// the parent's <c>CausationId</c> → domain events inherit → audit entries inherit.
/// The MediatR pipeline injects this context if missing; it must never be null at handler execution time.
/// </remarks>
public interface IOperationContext
{
    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.
    /// Generated once at the API boundary; constant for the entire operation tree.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.
    /// Null only for root operations. MUST be inherited, never regenerated for child operations.</summary>
    Guid? CausationId { get; }

    /// <summary>Gets the inbound API request identifier.</summary>
    Guid RequestId { get; }

    /// <summary>Gets the distributed trace identifier for OpenTelemetry propagation.</summary>
    string TraceId { get; }

    /// <summary>Gets the current span identifier for OpenTelemetry.</summary>
    string SpanId { get; }

    /// <summary>Gets the upstream workflow step identifier for long-running saga tracking.</summary>
    string? ParentOperationId { get; }

    /// <summary>Gets the tenant identifier for multi-tenancy context.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the site identifier if the operation is site-scoped.</summary>
    Guid? SiteId { get; }

    /// <summary>Gets the UTC timestamp when the root operation began.</summary>
    DateTime OperationStartedAtUtc { get; }
}
