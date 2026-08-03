// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Infrastructure
//  File:           WorkOrderEventReplayDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.4 Replay/Rebuild Across Systems, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper implementation of <see cref="IWorkOrderEventReplayRepository"/>, reading
//                  directly from <c>[System].[OutboxMessages]</c> -- the same table
//                  <see cref="BackgroundJobs.OutboxProcessor"/> publishes from -- via raw
//                  parameterised SQL (no stored procedure), matching that class's own established
//                  precedent for querying this system-owned table directly rather than through a
//                  per-module stored procedure. Filters to this WorkOrder's four lifecycle
//                  EventTypes and to the specific WorkOrderId via <c>JSON_VALUE</c> against the
//                  camelCase-serialized payload (see JsonSerializerConfiguration.Default:
//                  CamelCase naming policy + ValidatingValueObjectJsonConverterFactory, which
//                  serializes the strong-typed WorkOrderId value object as
//                  <c>{"workOrderId":{"value":"&lt;guid&gt;"}}</c>). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Infrastructure.Operational.WorkOrders.Persistence;

/// <summary>Dapper-backed <see cref="IWorkOrderEventReplayRepository"/> reading directly from <c>[System].[OutboxMessages]</c>.</summary>
public sealed class WorkOrderEventReplayDapperRepository(ISqlConnectionFactory connectionFactory) : IWorkOrderEventReplayRepository
{
    private static readonly string[] WorkOrderEventTypes =
    [
        "WorkOrderCreated",
        "WorkOrderAssigned",
        "WorkOrderStatusChanged",
        "WorkOrderCompleted",
    ];

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkOrderReplayEventRecord>> GetRawEventsAsync(
        Guid tenantId, Guid workOrderId, CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateReadConnection();

        var rows = await conn.QueryAsync<OutboxReplayRow>(
            new CommandDefinition(
                commandText: $"""
                    SELECT EventType, OccurredAt AS OccurredAtUtc, Payload AS PayloadJson
                    FROM {SqlConstants.OutboxTable}
                    WHERE TenantId = @TenantId
                      AND EventType IN @EventTypes
                      AND JSON_VALUE(Payload, '$.workOrderId.value') = @WorkOrderId
                    ORDER BY OccurredAt ASC
                    """,
                parameters: new
                {
                    TenantId = tenantId.ToString(),
                    EventTypes = WorkOrderEventTypes,
                    WorkOrderId = workOrderId.ToString(),
                },
                cancellationToken: cancellationToken));

        return rows.Select(r => new WorkOrderReplayEventRecord(r.EventType, r.OccurredAtUtc, r.PayloadJson)).ToList();
    }

    private sealed class OutboxReplayRow
    {
        public string EventType { get; init; } = string.Empty;
        public DateTime OccurredAtUtc { get; init; }
        public string PayloadJson { get; init; } = string.Empty;
    }
}
