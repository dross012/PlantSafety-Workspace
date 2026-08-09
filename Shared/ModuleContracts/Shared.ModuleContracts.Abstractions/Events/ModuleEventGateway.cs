// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Events
//  File:           ModuleEventGateway.cs
//  Created:        2026-08-09
//  Description:    In-memory IModuleEventGateway. Registered Singleton --
//                  subscriptions live for the process lifetime. Handlers
//                  for one event type run sequentially, in subscription
//                  order; a handler that throws does not prevent later
//                  handlers of the same publish from running (each is
//                  wrapped so one bad subscriber can't break the others),
//                  but the exception is rethrown after every handler has
//                  had a chance to run, as an AggregateException if more
//                  than one failed.
// ============================================================================
using System.Collections.Concurrent;

namespace Shared.ModuleContracts.Abstractions.Events;

/// <summary>In-memory, process-lifetime IModuleEventGateway.</summary>
public sealed class ModuleEventGateway : IModuleEventGateway
{
    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, Task>>> _subscribers = new();
    private readonly object _sync = new();

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent moduleEvent, CancellationToken cancellationToken = default) where TEvent : IModuleEvent
    {
        ArgumentNullException.ThrowIfNull(moduleEvent);

        List<Func<object, CancellationToken, Task>> handlers;
        lock (_sync)
        {
            handlers = _subscribers.TryGetValue(typeof(TEvent), out List<Func<object, CancellationToken, Task>>? existing)
                ? [.. existing]
                : [];
        }

        List<Exception> failures = [];
        foreach (Func<object, CancellationToken, Task> handler in handlers)
        {
            try
            {
                await handler(moduleEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        }

        if (failures.Count == 1)
        {
            throw failures[0];
        }

        if (failures.Count > 1)
        {
            throw new AggregateException($"{failures.Count} subscriber(s) failed handling {typeof(TEvent).Name}.", failures);
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IModuleEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        Func<object, CancellationToken, Task> wrapped = (evt, ct) => handler((TEvent)evt, ct);

        lock (_sync)
        {
            List<Func<object, CancellationToken, Task>> list = _subscribers.GetOrAdd(typeof(TEvent), _ => []);
            list.Add(wrapped);
        }

        return new Subscription(() =>
        {
            lock (_sync)
            {
                if (_subscribers.TryGetValue(typeof(TEvent), out List<Func<object, CancellationToken, Task>>? list))
                {
                    list.Remove(wrapped);
                }
            }
        });
    }

    private sealed class Subscription(Action onDispose) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            onDispose();
        }
    }
}
