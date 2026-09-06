// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           SqlEventSubscriberFactory.cs
//  Created:        2026-07-27
// ============================================================================
using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.EventBus.Abstractions.Handlers;
using Shared.EventBus.Infrastructure.Options;

namespace Shared.EventBus.Infrastructure.Dispatch;

/// <inheritdoc cref="IEventSubscriberFactory"/>
public sealed class SqlEventSubscriberFactory : IEventSubscriberFactory
{
    private readonly ConcurrentDictionary<string, IEventSubscriber> _cache = new(StringComparer.Ordinal);
    private readonly EventHandlerRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<EventBusOptions> _options;
    private readonly ILoggerFactory _loggerFactory;

    public SqlEventSubscriberFactory(
        EventHandlerRegistry registry,
        IServiceScopeFactory scopeFactory,
        IOptions<EventBusOptions> options,
        ILoggerFactory loggerFactory)
    {
        _registry = registry;
        _scopeFactory = scopeFactory;
        _options = options;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public IEventSubscriber Create(string consumerGroup)
        => _cache.GetOrAdd(consumerGroup, group => new SqlEventSubscriber(
            group, _registry, _scopeFactory, _options, _loggerFactory.CreateLogger<SqlEventSubscriber>()));
}
