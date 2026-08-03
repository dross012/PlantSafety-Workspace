// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.DeadLetter
//  Layer:          Infrastructure
//  File:           DeadLetterEventDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IDeadLetterEventRepository. Reads/writes
//                  [System].[DeadLetterEvents] via stored procedures only (Repositories
//                  Architectural Rule §2.10); no dynamic SQL. Mirrors OutboxWriter/
//                  IdempotentConsumerBase in going through ISqlConnectionFactory directly (this is
//                  a cross-tenant [System] system table with no RLS predicate, same reasoning as
//                  those two). ReplayAsync resolves the faulted payload's concrete CLR type via
//                  OutboxEventTypeRegistry (the same registry OutboxProcessor uses) and re-publishes
//                  it onto the bus via IBus.Publish before marking the row replayed -- this is the
//                  one method on this repository with a transport dependency (IBus), which is why
//                  replay is modelled as a repository operation rather than a plain Application-layer
//                  concern (see IDeadLetterEventRepository remarks). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Messaging.Models;
using PlantSafety.Application.Messaging.Repositories;

namespace PlantSafety.Infrastructure.Messaging.DeadLetter;

/// <summary>Dapper-backed repository over <c>[System].[DeadLetterEvents]</c>.</summary>
public sealed class DeadLetterEventDapperRepository(
    ISqlConnectionFactory connectionFactory,
    IServiceProvider serviceProvider,
    ILogger<DeadLetterEventDapperRepository> logger)
    : IDeadLetterEventRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DeadLetterEventDto>> GetRecentAsync(
        string? tenantId, string? eventType, bool includeReplayed, int top, int skip,
        CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("EventType", eventType);
        parameters.Add("IncludeReplayed", includeReplayed);
        parameters.Add("Top", top);
        parameters.Add("Skip", skip);

        var rows = await conn.QueryAsync<DeadLetterEventRow>(
            new CommandDefinition(
                "[System].[usp_DeadLetterEvents_GetRecent]",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DeadLetterEventDto>> GetByCorrelationIdAsync(
        Guid correlationId, CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();

        var rows = await conn.QueryAsync<DeadLetterEventRow>(
            new CommandDefinition(
                "[System].[usp_DeadLetterEvents_GetByCorrelationId]",
                new { CorrelationId = correlationId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DeadLetterEventsSummaryDto>> GetSummaryAsync(
        string? tenantId, int lookbackDays, CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("LookbackDays", lookbackDays);

        var rows = await conn.QueryAsync<DeadLetterSummaryRow>(
            new CommandDefinition(
                "[System].[usp_DeadLetterEvents_GetCountByEventType]",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.Select(r => new DeadLetterEventsSummaryDto(
            r.EventType, r.DeadLetterCount, r.OldestFaultedAt, r.NewestFaultedAt, r.OldestAgeHours, r.AvgRetryCount)).ToList();
    }

    /// <inheritdoc/>
    public async Task<DeadLetterEventDto?> GetByIdAsync(Guid deadLetterId, CancellationToken cancellationToken)
    {
        // No dedicated "get by id" procedure exists (yet); the recent-events procedure supports
        // every filter GetByIdAsync needs via a 1-row page, avoiding a near-duplicate procedure for
        // a single-row lookup used only by ReplayAsync below.
        using var conn = connectionFactory.CreateConnection();

        var rows = await conn.QueryAsync<DeadLetterEventRow>(
            new CommandDefinition(
                "[System].[usp_DeadLetterEvents_GetRecent]",
                new { TenantId = (string?)null, EventType = (string?)null, IncludeReplayed = true, Top = 500, Skip = 0 },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var match = rows.FirstOrDefault(r => r.DeadLetterId == deadLetterId);
        return match is null ? null : MapToDto(match);
    }

    /// <inheritdoc/>
    public async Task<bool> ReplayAsync(Guid deadLetterId, Guid replayedByUserId, CancellationToken cancellationToken)
    {
        var row = await GetByIdAsync(deadLetterId, cancellationToken);
        if (row is null)
        {
            logger.LogWarning("DeadLetterEventDapperRepository.ReplayAsync: DeadLetterId={DeadLetterId} not found.", deadLetterId);
            return false;
        }

        if (row.IsReplayed)
        {
            logger.LogWarning("DeadLetterEventDapperRepository.ReplayAsync: DeadLetterId={DeadLetterId} already replayed at {ReplayedAtUtc}.",
                deadLetterId, row.ReplayedAtUtc);
            return false;
        }

        await PublishAsync(row, cancellationToken);

        using var conn = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("DeadLetterId", deadLetterId);
        parameters.Add("ReplayedByUserId", replayedByUserId);
        parameters.Add("ReplayedAtUtc", (DateTime?)null);

        var rowsAffected = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "[System].[usp_DeadLetterEvents_MarkReplayed]",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        logger.LogInformation(
            "DeadLetterEventDapperRepository.ReplayAsync: DeadLetterId={DeadLetterId} EventType={EventType} republished and marked replayed by {ReplayedByUserId}.",
            deadLetterId, row.EventType, replayedByUserId);

        return rowsAffected > 0;
    }

    private async Task PublishAsync(DeadLetterEventDto row, CancellationToken cancellationToken)
    {
        // Resolved per-call to avoid capturing a stale bus instance, mirroring
        // OutboxProcessor.PublishAsync's identical per-scope resolution pattern.
        using var scope = serviceProvider.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var correlationId = row.CorrelationId?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(row.Payload))
        {
            throw new MessagingException(row.EventType, correlationId,
                new InvalidOperationException("Cannot replay a dead-letter row with no captured payload."));
        }

        if (OutboxEventTypeRegistry.TryResolve(row.EventType, out var domainEventType) && domainEventType is not null)
        {
            var domainEvent = JsonSerializer.Deserialize(row.Payload, domainEventType, JsonSerializerConfiguration.Default)
                ?? throw new MessagingException(row.EventType, correlationId,
                    new InvalidOperationException("Deserialized dead-letter payload was null."));

            await bus.Publish(domainEvent, domainEventType, ctx =>
            {
                if (row.TenantId is not null) ctx.Headers.Set(OutboxHeaderNames.TenantId, row.TenantId);
                if (row.ActorId is not null) ctx.Headers.Set(OutboxHeaderNames.ActorId, row.ActorId);
                if (row.CorrelationId is not null) ctx.Headers.Set(OutboxHeaderNames.CorrelationId, row.CorrelationId.Value.ToString());
                if (row.MessageId is not null) ctx.Headers.Set(OutboxHeaderNames.OutboxMessageId, row.MessageId.Value.ToString());
            }, cancellationToken);
            return;
        }

        // Event type could not be resolved back to a concrete CLR type (e.g. the original fault
        // occurred before OutboxEventTypeRegistry could resolve it, or the message predates a
        // module rename). Degrade to publishing the raw JSON payload wrapped in the generic
        // envelope shape rather than silently failing the replay.
        logger.LogWarning(
            "DeadLetterEventDapperRepository.ReplayAsync: no registered CLR type for EventType={EventType}; publishing raw payload as fallback.",
            row.EventType);

        await bus.Publish(new UnresolvedDeadLetterReplayEnvelope(row.EventType, row.SchemaVersion, row.Payload), cancellationToken);
    }

    private static DeadLetterEventDto MapToDto(DeadLetterEventRow r) => new(
        r.DeadLetterId, r.MessageId, r.EventType, r.SchemaVersion, r.TenantId, r.Payload, r.FaultReason,
        r.ExceptionDetail, r.RetryCount, r.FaultedAt, r.CreatedAt, r.CorrelationId, r.CausationId, r.ActorId,
        r.ConsumerName, r.ReplayedAtUtc, r.ReplayedByUserId);

    /// <summary>Raw projection of a <c>[System].[DeadLetterEvents]</c> row before DTO mapping.</summary>
    private sealed class DeadLetterEventRow
    {
        public Guid DeadLetterId { get; init; }
        public Guid? MessageId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public string SchemaVersion { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public string? Payload { get; init; }
        public string FaultReason { get; init; } = string.Empty;
        public string? ExceptionDetail { get; init; }
        public int RetryCount { get; init; }
        public DateTime FaultedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public Guid? CorrelationId { get; init; }
        public Guid? CausationId { get; init; }
        public string? ActorId { get; init; }
        public string? ConsumerName { get; init; }
        public DateTime? ReplayedAtUtc { get; init; }
        public Guid? ReplayedByUserId { get; init; }
    }

    /// <summary>Raw projection of a <c>usp_DeadLetterEvents_GetCountByEventType</c> row.</summary>
    private sealed class DeadLetterSummaryRow
    {
        public string EventType { get; init; } = string.Empty;
        public int DeadLetterCount { get; init; }
        public DateTime OldestFaultedAt { get; init; }
        public DateTime NewestFaultedAt { get; init; }
        public int OldestAgeHours { get; init; }
        public double AvgRetryCount { get; init; }
    }
}

/// <summary>
/// Fallback replay envelope published when a dead-lettered event's <c>EventType</c> cannot be
/// resolved back to a concrete CLR type (see <see cref="DeadLetterEventDapperRepository.PublishAsync"/>).
/// Not consumed by any existing consumer; exists so the replay attempt is observable on the bus
/// rather than silently dropped.
/// </summary>
/// <param name="EventType">The original, unresolved event-type discriminator.</param>
/// <param name="SchemaVersion">The original event envelope schema edition.</param>
/// <param name="Payload">The original raw JSON payload.</param>
public sealed record UnresolvedDeadLetterReplayEnvelope(string EventType, string SchemaVersion, string Payload);
