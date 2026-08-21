// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           InMemoryEventBus.cs
//  Created:        2026-07-27
//  Description:    In-memory IEventPublisher + IEventSubscriber for unit/integration tests
//                  and local development without a SQL Server instance. Mirrors SqlEventQueue
//                  semantics closely enough for contract tests: idempotency (ProcessedEvents
//                  emulated with an in-memory HashSet), retry/backoff counters, and a dead-
//                  letter list -- but is NOT durable and NOT for production use.
// ============================================================================
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Abstractions.Handlers;
using Shared.EventBus.Infrastructure.Dispatch;
using Shared.EventBus.Infrastructure.Options;

namespace Shared.EventBus.Infrastructure.InMemory;

/// <summary>Represents one dead-lettered event captured by <see cref="InMemoryEventBus"/>.</summary>
public sealed record InMemoryDeadLetter(string ConsumerGroup, EventEnvelope Envelope, int AttemptCount, string FailureReason, bool IsPoisonMessage);

/// <summary>
/// In-memory, single-process implementation of <see cref="IEventPublisher"/> and a
/// per-consumer-group <see cref="IEventSubscriber"/> factory, for tests and local dev.
/// </summary>
public sealed class InMemoryEventBus : IEventPublisher
{
    private readonly ConcurrentQueue<EventEnvelope> _queue = new();
    private readonly ConcurrentDictionary<(string ConsumerGroup, Guid EventId), byte> _processed = new();
    private readonly ConcurrentDictionary<(string ConsumerGroup, Guid EventId), (int AttemptCount, string? LastError)> _retryState = new();
    private readonly ConcurrentBag<InMemoryDeadLetter> _deadLetters = new();
    private readonly EventHandlerRegistry _registry;
    private readonly EventBusOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _rootServices;

    public InMemoryEventBus(EventHandlerRegistry registry, EventBusOptions options, ILoggerFactory loggerFactory, IServiceProvider rootServices)
    {
        _registry = registry;
        _options = options;
        _loggerFactory = loggerFactory;
        _rootServices = rootServices;
    }

    /// <summary>All envelopes published so far, in publish order. Test/inspection convenience.</summary>
    public IReadOnlyCollection<EventEnvelope> PublishedEvents => _queue.ToArray();

    /// <summary>All dead-lettered envelopes captured across all consumer groups.</summary>
    public IReadOnlyCollection<InMemoryDeadLetter> DeadLetters => _deadLetters.ToArray();

    /// <inheritdoc/>
    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(envelope);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task PublishAsync<TPayload>(
        TPayload payload, string tenantId, Guid correlationId, Guid causationId, DateTime occurredAtUtc,
        string sourceModule, string eventType, int schemaVersion, CancellationToken cancellationToken = default)
        where TPayload : notnull
    {
        var envelope = EventEnvelope.Create(payload, tenantId, correlationId, causationId, occurredAtUtc, sourceModule, eventType, schemaVersion);
        return PublishAsync(envelope, cancellationToken);
    }

    /// <summary>Creates an in-memory subscriber for the given consumer group.</summary>
    public IEventSubscriber CreateSubscriber(string consumerGroup)
        => new InMemorySubscriber(consumerGroup, this, _registry, _options, _loggerFactory.CreateLogger<InMemorySubscriber>(), _rootServices);

    private sealed class InMemorySubscriber : IEventSubscriber
    {
        private readonly InMemoryEventBus _bus;
        private readonly EventHandlerRegistry _registry;
        private readonly EventBusOptions _options;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public string ConsumerGroup { get; }

        public InMemorySubscriber(string consumerGroup, InMemoryEventBus bus, EventHandlerRegistry registry, EventBusOptions options, ILogger logger, IServiceProvider services)
        {
            ConsumerGroup = consumerGroup;
            _bus = bus;
            _registry = registry;
            _options = options;
            _logger = logger;
            _services = services;
        }

        public async Task<int> ProcessDueEventsAsync(int maxBatchSize = 50, CancellationToken cancellationToken = default)
        {
            var processed = 0;
            foreach (var envelope in _bus._queue)
            {
                if (processed >= maxBatchSize) break;
                var key = (ConsumerGroup, envelope.EventId);
                if (_bus._processed.ContainsKey(key)) continue;

                var descriptor = _registry.Find(ConsumerGroup, envelope.PayloadType);
                if (descriptor is null)
                {
                    _bus._processed[key] = 1;
                    processed++;
                    continue;
                }

                try
                {
                    await descriptor.Invoke(_services, envelope, cancellationToken);
                    _bus._processed[key] = 1;
                    _bus._retryState.TryRemove(key, out _);
                    _logger.LogInformation("InMemoryEventBus[{ConsumerGroup}]: handled {EventType} (EventId={EventId}). Outcome=Success.", ConsumerGroup, envelope.EventType, envelope.EventId);
                }
                catch (Exception ex)
                {
                    var (prevAttempts, prevError) = _bus._retryState.TryGetValue(key, out var st) ? st : (0, null);
                    var attempts = prevAttempts + 1;
                    var isRepeated = prevError == ex.Message;
                    _bus._retryState[key] = (attempts, ex.Message);

                    if (attempts >= _options.MaxDeliveryAttempts)
                    {
                        _bus._deadLetters.Add(new InMemoryDeadLetter(
                            ConsumerGroup, envelope, attempts, ex.Message,
                            isRepeated && attempts >= _options.PoisonMessageRepeatThreshold));
                        _bus._processed[key] = 1;
                        _bus._retryState.TryRemove(key, out _);
                        _logger.LogError(ex, "InMemoryEventBus[{ConsumerGroup}]: {EventType} (EventId={EventId}) dead-lettered after {Attempts} attempts. Outcome=DeadLettered.", ConsumerGroup, envelope.EventType, envelope.EventId, attempts);
                    }
                    else
                    {
                        _logger.LogWarning(ex, "InMemoryEventBus[{ConsumerGroup}]: failed {EventType} (EventId={EventId}), attempt {Attempts}. Outcome=Failed.", ConsumerGroup, envelope.EventType, envelope.EventId, attempts);
                    }
                }

                processed++;
            }

            return processed;
        }
    }
}
