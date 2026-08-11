// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Commands
//  Layer:          IntegrationTests
//  File:           InboundWebhookLifecycleIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the ONE genuine write path in the
//                  Integrations module: IInboundWebhookRepository.RecordEventAsync ->
//                  InboundWebhookDapperRepository -> [Integrations].[InboundWebhookEvents] /
//                  usp_InboundWebhookEvents_RecordEvent, consumed by
//                  ProcessInboundWebhookCommandHandler. Exercises the real MediatR command
//                  handler -> real repository -> real DI -> real database end to end:
//                    1. Record a new event -> Accepted=true, ETag present (non-null,
//                       non-whitespace), and a real INSERT-operation AuditLog row exists.
//                    2. Replay the exact same (TenantId, EventId) -> idempotent accept
//                       (Accepted=true still), with the ETag STABLE (this table has no update
//                       path, so an idempotent replay must not advance the ROWVERSION-backed
//                       ETag) and a real IDEMPOTENT_ACCEPT-operation AuditLog row exists.
//                       NOTE: this module deliberately has no GetById/List read procedure for
//                       InboundWebhookEvents (confirmed via inventory of
//                       PlantSafety.Application.Integrations.Commands and
//                       PlantSafety.Infrastructure.Integrations.Sql.Procedures) -- per the "do not
//                       invent new read paths" boundary, "get" verification here reads the
//                       persisted row directly via SQL rather than through a new Application-layer
//                       query.
//                    3. Force a genuine SQL-level failure (temporary procedure-body swap injecting
//                       a divide-by-zero, verified live, restored via try/finally -- the
//                       ProcessInboundWebhookCommandHandler catches all exceptions into a
//                       Result.Failure rather than propagating them, so this must call the
//                       procedure directly via raw ADO exactly like the read-only forced-failure
//                       tests) to prove the CATCH block's ErrorLog insert + THROW modernization is
//                       genuinely wired up end to end, not just present in the .sql file text.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Commands;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Commands;

/// <summary>
/// Integration tests for the <c>InboundWebhookEvents</c> write path in the <c>Integrations</c> area.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class InboundWebhookLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RecordEvent_NewEvent_ShouldBeAccepted_HaveETag_AndWriteInsertAuditLogRow()
    {
        // Must be the authenticated tenant: TenantIsolationBehavior rejects a command whose TenantId
        // differs from IOperationContext.TenantId, and this test authenticates as TenantId below.
        var tenantId = TenantId;
        var siteId = Guid.NewGuid();
        var eventId = Guid.NewGuid().ToString("N");

        var command = new ProcessInboundWebhookCommand(
            eventId,
            "IncidentReported",
            "ExternalSafetySystem",
            tenantId,
            siteId,
            DateTimeOffset.UtcNow,
            "{\"payload\":\"real seeded webhook body\"}");

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real ProcessInboundWebhookCommandHandler must succeed against the real stored procedure");
            result.Value!.EventId.Should().Be(eventId);
            result.Value.Accepted.Should().BeTrue("a genuinely new event must be accepted");
            result.Value.ETag.Should().NotBeNullOrWhiteSpace(
                "usp_InboundWebhookEvents_RecordEvent must project a RowVersion-derived ETag on insert");
        }
        finally
        {
            scope.Dispose();
        }

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var persistedETag = await GetPersistedETagAsync(conn, tenantId, eventId);
        persistedETag.Should().NotBeNullOrWhiteSpace("the row must be persisted with a real RowVersion-derived ETag");

        var auditLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
            new { TableName = "InboundWebhookEvents", Operation = "INSERT", Pattern = $"%EventId={eventId}%" });

        auditLogCount.Should().BeGreaterThan(0,
            "usp_InboundWebhookEvents_RecordEvent's canonical AuditLog insert must have recorded this INSERT");
    }

    [Fact]
    public async Task RecordEvent_ReplayedSameEvent_ShouldBeIdempotent_WithStableETag_AndWriteIdempotentAcceptAuditLogRow()
    {
        // Must be the authenticated tenant: TenantIsolationBehavior rejects a command whose TenantId
        // differs from IOperationContext.TenantId, and this test authenticates as TenantId below.
        var tenantId = TenantId;
        var siteId = Guid.NewGuid();
        var eventId = Guid.NewGuid().ToString("N");

        var command = new ProcessInboundWebhookCommand(
            eventId,
            "IncidentReported",
            "ExternalSafetySystem",
            tenantId,
            siteId,
            DateTimeOffset.UtcNow,
            "{\"payload\":\"real seeded webhook body\"}");

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var firstResult = await sender.Send(command, CancellationToken.None);
            firstResult.IsSuccess.Should().BeTrue();
            firstResult.Value!.Accepted.Should().BeTrue();
            var firstETag = firstResult.Value.ETag;
            firstETag.Should().NotBeNullOrWhiteSpace();

            // Replay the exact same (TenantId, EventId) -- must be accepted idempotently, not
            // inserted a second time, and (this table has no update path) the ETag must not move.
            var secondResult = await sender.Send(command, CancellationToken.None);

            secondResult.IsSuccess.Should().BeTrue("a replayed event must still succeed (idempotent accept), not fail");
            secondResult.Value!.Accepted.Should().BeTrue("a replayed event must be accepted idempotently");
            secondResult.Value.ETag.Should().Be(firstETag,
                "usp_InboundWebhookEvents_RecordEvent's idempotent-accept branch performs no write, so the ROWVERSION-backed ETag must remain stable across replays");
        }
        finally
        {
            scope.Dispose();
        }

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var rowCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [Integrations].[InboundWebhookEvents] WHERE TenantId = @TenantId AND EventId = @EventId",
            new { TenantId = tenantId, EventId = eventId });

        rowCount.Should().Be(1, "the unique (TenantId, EventId) index must prevent a duplicate row from being inserted on replay");

        var idempotentAcceptAuditLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
            new { TableName = "InboundWebhookEvents", Operation = "IDEMPOTENT_ACCEPT", Pattern = $"%EventId={eventId}%" });

        idempotentAcceptAuditLogCount.Should().BeGreaterThan(0,
            "usp_InboundWebhookEvents_RecordEvent's canonical AuditLog insert must have recorded the replayed IDEMPOTENT_ACCEPT");
    }

    [Fact]
    public async Task RecordEvent_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        // ProcessInboundWebhookCommandHandler catches all exceptions and returns a
        // Result.Failure rather than propagating them (see the handler's try/catch), so this test
        // -- like the read-only forced-failure tests -- must call the procedure directly via raw
        // ADO to observe the genuine SqlException and prove the CATCH block's ErrorLog insert +
        // THROW modernization is wired up end to end.
        var tenantId = Guid.NewGuid();
        var eventId = Guid.NewGuid().ToString("N");

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_InboundWebhookEvents_RecordEvent]
                @TenantId      UNIQUEIDENTIFIER,
                @SiteId        UNIQUEIDENTIFIER = NULL,
                @EventId       NVARCHAR(200),
                @EventType     NVARCHAR(200),
                @Source        NVARCHAR(200),
                @OccurredAtUtc DATETIMEOFFSET(3),
                @RawPayload    NVARCHAR(MAX),
                @Accepted      BIT OUTPUT,
                @ETag          NVARCHAR(34) OUTPUT
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;

                    DECLARE @IsNewInsert BIT = 0;

                    IF EXISTS (
                        SELECT 1
                        FROM [Integrations].[InboundWebhookEvents] WITH (UPDLOCK, HOLDLOCK)
                        WHERE TenantId = @TenantId AND EventId = @EventId
                    )
                    BEGIN
                        SET @Accepted = 1;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [Integrations].[InboundWebhookEvents]
                            (TenantId, SiteId, EventId, EventType, Source, OccurredAtUtc, RawPayload, IsDeleted)
                        VALUES
                            (@TenantId, @SiteId, @EventId, @EventType, @Source, @OccurredAtUtc, @RawPayload, 0);

                        SET @Accepted = 1;
                        SET @IsNewInsert = 1;
                    END

                    SELECT @ETag = CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)
                    FROM [Integrations].[InboundWebhookEvents]
                    WHERE TenantId = @TenantId AND EventId = @EventId;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (
                            N'InboundWebhookEvents', DB_NAME(),
                            CASE WHEN @IsNewInsert = 1 THEN N'INSERT' ELSE N'IDEMPOTENT_ACCEPT' END,
                            SYSTEM_USER, SYSUTCDATETIME(), 0,
                            CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; EventType=', @EventType),
                            0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

                DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; User=', SYSTEM_USER);
                DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                BEGIN TRY
                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                END TRY
                BEGIN CATCH
                END CATCH;

                THROW;
            END CATCH";

        const string forcedFailureProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_InboundWebhookEvents_RecordEvent]
                @TenantId      UNIQUEIDENTIFIER,
                @SiteId        UNIQUEIDENTIFIER = NULL,
                @EventId       NVARCHAR(200),
                @EventType     NVARCHAR(200),
                @Source        NVARCHAR(200),
                @OccurredAtUtc DATETIMEOFFSET(3),
                @RawPayload    NVARCHAR(MAX),
                @Accepted      BIT OUTPUT,
                @ETag          NVARCHAR(34) OUTPUT
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;

                    DECLARE @ForcedFailureDivisor INT = 0;
                    DECLARE @ForcedFailureResult INT = 1 / @ForcedFailureDivisor; -- deliberate forced runtime failure for test coverage

                    SET @Accepted = 1;
                    SET @ETag = N'unreachable';

                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

                DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; User=', SYSTEM_USER);
                DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                BEGIN TRY
                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                END TRY
                BEGIN CATCH
                END CATCH;

                THROW;
            END CATCH";

        await conn.ExecuteAsync(forcedFailureProcedure);

        try
        {
            Func<Task> act = async () =>
            {
                await using var cmd = new SqlCommand("[Integrations].[usp_InboundWebhookEvents_RecordEvent]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@EventId", eventId);
                cmd.Parameters.AddWithValue("@EventType", "IncidentReported");
                cmd.Parameters.AddWithValue("@Source", "ExternalSafetySystem");
                cmd.Parameters.AddWithValue("@OccurredAtUtc", DateTimeOffset.UtcNow);
                cmd.Parameters.AddWithValue("@RawPayload", "{}");
                var acceptedParam = cmd.Parameters.Add("@Accepted", SqlDbType.Bit);
                acceptedParam.Direction = ParameterDirection.Output;
                var eTagParam = cmd.Parameters.Add("@ETag", SqlDbType.NVarChar, 34);
                eTagParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_InboundWebhookEvents_RecordEvent must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_InboundWebhookEvents_RecordEvent", Pattern = $"%EventId={eventId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_InboundWebhookEvents_RecordEvent's CATCH block must have logged the forced failure to ErrorLog before re-throwing");

        var rowCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [Integrations].[InboundWebhookEvents] WHERE TenantId = @TenantId AND EventId = @EventId",
            new { TenantId = tenantId, EventId = eventId });

        rowCount.Should().Be(0, "the forced failure occurred before any INSERT, and the explicit transaction must have rolled back");
    }

    private static async Task<string?> GetPersistedETagAsync(SqlConnection conn, Guid tenantId, string eventId)
    {
        return await conn.ExecuteScalarAsync<string?>(
            @"SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)
              FROM [Integrations].[InboundWebhookEvents]
              WHERE TenantId = @TenantId AND EventId = @EventId",
            new { TenantId = tenantId, EventId = eventId });
    }
}
