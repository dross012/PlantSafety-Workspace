// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           IEventSubscriberFactory.cs
//  Created:        2026-07-27
//  Description:    Creates an IEventSubscriber scoped to one consumer group. Consuming
//                  modules (e.g. Reporting) resolve this from DI and create one subscriber
//                  per consumer group they own, then drive ProcessDueEventsAsync from a
//                  background worker / hosted service / scheduled job.
// ============================================================================
using Shared.EventBus.Abstractions.Handlers;

namespace Shared.EventBus.Infrastructure.Dispatch;

/// <summary>Creates <see cref="IEventSubscriber"/> instances bound to a specific consumer group.</summary>
public interface IEventSubscriberFactory
{
    /// <summary>Creates (or returns a cached) subscriber for the given consumer group.</summary>
    IEventSubscriber Create(string consumerGroup);
}
