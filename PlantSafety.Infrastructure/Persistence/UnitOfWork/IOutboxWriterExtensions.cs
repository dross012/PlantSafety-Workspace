// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           IOutboxWriterExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Enqueue overload on IOutboxWriter to accept actor context fields.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.UnitOfWork;

/// <summary>Extension that adds the <c>Enqueue</c> method to <see cref="IOutboxWriter"/>.</summary>
public static class IOutboxWriterExtensions
{
    /// <summary>
    /// Enqueues a domain event alongside its canonical actor context fields.
    /// </summary>
    public static void Enqueue(
        this IOutboxWriter writer,
        IDomainEvent domainEvent,
        string tenantId,
        string actorId,
        string correlationId)
    {
        // OutboxWriter implements this directly; extension is for interface callers
        // that need to call Enqueue without casting.
        if (writer is OutboxWriter ow)
        {
            ow.Enqueue(domainEvent, tenantId, actorId, correlationId);
        }
    }
}
