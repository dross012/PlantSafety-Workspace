// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           SqlEventPublisher.cs
//  Created:        2026-07-27
//  Description:    IEventPublisher backed by dbo.EventQueue (Dapper, raw parameterised SQL --
//                  no stored procedure needed for a single-table INSERT, consistent with
//                  Shared.AuditLogging.Infrastructure's own connection-per-call pattern for
//                  cross-database/cross-module shared tables). No EF, no ORM.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Infrastructure.Exceptions;
using Shared.EventBus.Infrastructure.Options;

namespace Shared.EventBus.Infrastructure.Persistence;

/// <summary><see cref="IEventPublisher"/> that durably enqueues envelopes into dbo.EventQueue.</summary>
public sealed class SqlEventPublisher : IEventPublisher
{
    private const string InsertSql = """
        INSERT INTO dbo.EventQueue
            (EventId, EventType, SchemaVersion, PayloadType, TenantId, CorrelationId, CausationId,
             OccurredAtUtc, SourceModule, Payload, EnqueuedAtUtc)
        VALUES
            (@EventId, @EventType, @SchemaVersion, @PayloadType, @TenantId, @CorrelationId, @CausationId,
             @OccurredAtUtc, @SourceModule, @Payload, @EnqueuedAtUtc);
        """;

    private readonly EventBusOptions _options;
    private readonly ILogger<SqlEventPublisher> _logger;

    public SqlEventPublisher(IOptions<EventBusOptions> options, ILogger<SqlEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        try
        {
            using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                InsertSql,
                new
                {
                    envelope.EventId,
                    envelope.EventType,
                    envelope.SchemaVersion,
                    envelope.PayloadType,
                    envelope.TenantId,
                    envelope.CorrelationId,
                    envelope.CausationId,
                    envelope.OccurredAtUtc,
                    envelope.SourceModule,
                    envelope.Payload,
                    envelope.EnqueuedAtUtc,
                },
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

            _logger.LogInformation(
                "Shared.EventBus: published {EventType} (EventId={EventId}, CorrelationId={CorrelationId}) to dbo.EventQueue.",
                envelope.EventType, envelope.EventId, envelope.CorrelationId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Shared.EventBus: failed to publish {EventType} (EventId={EventId}).", envelope.EventType, envelope.EventId);
            throw new EventBusException($"Failed to publish event {envelope.EventType} ({envelope.EventId}) to dbo.EventQueue.", ex);
        }
    }

    /// <inheritdoc/>
    public Task PublishAsync<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime occurredAtUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        CancellationToken cancellationToken = default)
        where TPayload : notnull
    {
        var envelope = EventEnvelope.Create(
            payload, tenantId, correlationId, causationId, occurredAtUtc, sourceModule, eventType, schemaVersion);
        return PublishAsync(envelope, cancellationToken);
    }
}
