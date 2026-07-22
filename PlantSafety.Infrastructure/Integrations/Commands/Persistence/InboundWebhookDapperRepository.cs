// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Commands
//  Layer:          Infrastructure
//  File:           InboundWebhookDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing IInboundWebhookRepository.RecordEventAsync.
//                  Calls [Integrations].[usp_InboundWebhookEvents_RecordEvent], an idempotent
//                  check-and-insert stored procedure keyed by (TenantId, EventId). Uses raw
//                  Dapper DynamicParameters (rather than StoredProcedureBuilder/DapperCommandExecutor)
//                  because this call requires a BIT OUTPUT parameter, which neither shared helper
//                  currently supports.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Commands;

namespace PlantSafety.Infrastructure.Integrations.Commands.Persistence;

/// <summary>
/// Dapper-backed repository recording inbound webhook events for idempotent, outbox-based
/// downstream processing. Executes a write stored procedure that performs the idempotent
/// check-and-insert.
/// </summary>
public sealed class InboundWebhookDapperRepository(
    ISqlConnectionFactory connectionFactory,
    ILogger<InboundWebhookDapperRepository> logger)
    : IInboundWebhookRepository
{
    /// <inheritdoc/>
    public async Task<ProcessInboundWebhookResult> RecordEventAsync(
        string eventId,
        string eventType,
        string source,
        Guid tenantId,
        Guid? siteId,
        DateTimeOffset occurredAt,
        string rawPayload,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "RecordEvent for Integrations inbound webhook {EventId} ({EventType}) tenant {TenantId}",
            eventId, eventType, tenantId);

        using var connection = connectionFactory.CreateConnection();

        await connection.SetTenantSessionContextAsync(tenantId, cancellationToken: cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId, DbType.Guid);
        parameters.Add("SiteId", siteId, DbType.Guid);
        parameters.Add("EventId", eventId, DbType.String, size: 200);
        parameters.Add("EventType", eventType, DbType.String, size: 200);
        parameters.Add("Source", source, DbType.String, size: 200);
        parameters.Add("OccurredAtUtc", occurredAt, DbType.DateTimeOffset);
        parameters.Add("RawPayload", rawPayload, DbType.String, size: -1);
        parameters.Add("Accepted", dbType: DbType.Boolean, direction: ParameterDirection.Output);

        try
        {
            await connection.ExecuteAsync(
                sql: "[Integrations].[usp_InboundWebhookEvents_RecordEvent]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "RecordEvent failed for inbound webhook {EventId}", eventId);
            throw new PersistenceException("[Integrations].[usp_InboundWebhookEvents_RecordEvent]", string.Empty, ex);
        }

        var accepted = parameters.Get<bool?>("Accepted") ?? false;

        return new ProcessInboundWebhookResult(eventId, accepted);
    }
}
