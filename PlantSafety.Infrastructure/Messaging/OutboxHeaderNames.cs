// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging
//  Layer:          Infrastructure
//  File:           OutboxHeaderNames.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3 Cross-Module Orchestration)
//  SchemaVersion:  2026.1
//  Description:    Canonical MassTransit message-header names used to propagate the outbox
//                  envelope's TenantId/ActorId/CorrelationId/MessageId alongside the concrete,
//                  strongly-typed domain event body (which -- for most pre-existing domain events
//                  -- does not itself carry TenantId). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging;

/// <summary>
/// Canonical header names set by <see cref="BackgroundJobs.OutboxProcessor"/> on every published
/// integration event and read back by cross-module consumers via <see cref="OutboxHeaderExtensions"/>.
/// </summary>
public static class OutboxHeaderNames
{
    /// <summary>Header carrying the outbox envelope's <c>TenantId</c>.</summary>
    public const string TenantId = "PlantSafety-TenantId";

    /// <summary>Header carrying the outbox envelope's <c>ActorId</c>.</summary>
    public const string ActorId = "PlantSafety-ActorId";

    /// <summary>Header carrying the outbox envelope's <c>CorrelationId</c>.</summary>
    public const string CorrelationId = "PlantSafety-CorrelationId";

    /// <summary>Header carrying the originating <c>System.OutboxMessages.MessageId</c>.</summary>
    public const string OutboxMessageId = "PlantSafety-OutboxMessageId";
}

/// <summary>Convenience accessors for reading canonical outbox headers off a <see cref="ConsumeContext"/>.</summary>
public static class OutboxHeaderExtensions
{
    /// <summary>Reads the propagated TenantId header, or <see cref="Guid.Empty"/> if absent/unparsable.</summary>
    public static Guid GetEnvelopeTenantId(this ConsumeContext context)
    {
        var raw = context.Headers.Get<string>(OutboxHeaderNames.TenantId);
        return Guid.TryParse(raw, out var value) ? value : Guid.Empty;
    }

    /// <summary>Reads the propagated ActorId header, or <see cref="Guid.Empty"/> if absent/unparsable.</summary>
    public static Guid GetEnvelopeActorId(this ConsumeContext context)
    {
        var raw = context.Headers.Get<string>(OutboxHeaderNames.ActorId);
        return Guid.TryParse(raw, out var value) ? value : Guid.Empty;
    }

    /// <summary>Reads the propagated CorrelationId header, generating a new one if absent/unparsable.</summary>
    public static Guid GetEnvelopeCorrelationId(this ConsumeContext context)
    {
        var raw = context.Headers.Get<string>(OutboxHeaderNames.CorrelationId);
        return Guid.TryParse(raw, out var value) ? value : Guid.NewGuid();
    }
}
