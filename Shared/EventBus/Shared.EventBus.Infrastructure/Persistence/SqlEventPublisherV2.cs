// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           SqlEventPublisherV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.4: IEventPublisherV2 backed by the SAME dbo.
//                  EventQueue table SqlEventPublisher (V1) uses, extended
//                  with the nullable FacilityId/IndustryName/Location/
//                  HazardType columns (see sql/Phase1-EventEnvelopeV2/00-
//                  alter-eventqueue-add-enrichment-columns.sql). One
//                  physical queue, two envelope shapes reading/writing it --
//                  exactly what "EventEnvelopeV2 must coexist with
//                  EventEnvelope (V1)" means at the storage layer: a V1
//                  row and a V2 row are indistinguishable except that a V2
//                  row may have its enrichment columns populated.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Infrastructure.Exceptions;
using Shared.EventBus.Infrastructure.Options;

namespace Shared.EventBus.Infrastructure.Persistence;

/// <summary><see cref="IEventPublisherV2"/> that durably enqueues V2 envelopes into dbo.EventQueue.</summary>
public sealed class SqlEventPublisherV2 : IEventPublisherV2
{
    private const string InsertSql = """
        INSERT INTO dbo.EventQueue
            (EventId, EventType, SchemaVersion, PayloadType, TenantId, CorrelationId, CausationId,
             OccurredAtUtc, SourceModule, Payload, EnqueuedAtUtc,
             FacilityId, IndustryName, Location, HazardType)
        VALUES
            (@EventId, @EventType, @SchemaVersion, @PayloadType, @TenantId, @CorrelationId, @CausationId,
             @TimestampUtc, @SourceModule, @Payload, @EnqueuedAtUtc,
             @FacilityId, @IndustryName, @Location, @HazardType);
        """;

    private readonly EventBusOptions _options;
    private readonly ILogger<SqlEventPublisherV2> _logger;

    public SqlEventPublisherV2(IOptions<EventBusOptions> options, ILogger<SqlEventPublisherV2> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(EventEnvelopeV2 envelope, CancellationToken cancellationToken = default)
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
                    envelope.TimestampUtc,
                    envelope.SourceModule,
                    envelope.Payload,
                    envelope.EnqueuedAtUtc,
                    envelope.FacilityId,
                    envelope.IndustryName,
                    envelope.Location,
                    envelope.HazardType,
                },
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

            _logger.LogInformation(
                "Shared.EventBus: published V2 {EventType} (EventId={EventId}, CorrelationId={CorrelationId}, FacilityId={FacilityId}) to dbo.EventQueue.",
                envelope.EventType, envelope.EventId, envelope.CorrelationId, envelope.FacilityId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Shared.EventBus: failed to publish V2 {EventType} (EventId={EventId}).", envelope.EventType, envelope.EventId);
            throw new EventBusException($"Failed to publish V2 event {envelope.EventType} ({envelope.EventId}) to dbo.EventQueue.", ex);
        }
    }

    /// <inheritdoc />
    public Task PublishAsync<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime timestampUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        Guid? facilityId = null,
        string? industryName = null,
        string? location = null,
        string? hazardType = null,
        CancellationToken cancellationToken = default)
        where TPayload : notnull
    {
        EventEnvelopeV2 envelope = EventEnvelopeV2.Create(
            payload, tenantId, correlationId, causationId, timestampUtc, sourceModule, eventType, schemaVersion,
            facilityId: facilityId, industryName: industryName, location: location, hazardType: hazardType);
        return PublishAsync(envelope, cancellationToken);
    }
}
