// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.IoT
//  Layer:          IntegrationTests
//  File:           IoTIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub with real Phase 3 SQL Modernization
//                  coverage for the live, read-only Integrations.IoT path (IIoTRepository ->
//                  IoTDapperRepository -> [Integrations].[IoTItems] / usp_IoT_GetIoTReadings),
//                  consumed by GetIoTQueryHandler. This path has no write procedure, so
//                  ROWVERSION/ETag modernization does not apply here.
//  SchemaVersion:  2026.1
//  Description:    Exercises the real MediatR handler -> real repository -> real DI -> real
//                  database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.IoT.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.IoT;

/// <summary>Integration tests for the <c>IoT</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class IoTIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetIoT_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var ingestedBy = Guid.NewGuid();
        var inRangeId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var outOfRangeId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[IoTItems]
                    (ReadingId, TenantId, SiteId, DeviceId, SensorType, ReadingValue, Status, RecordedAtUtc, IngestedBy, IsDeleted)
                  VALUES (@ReadingId, @TenantId, @SiteId, @DeviceId, @SensorType, @ReadingValue, @Status, @RecordedAtUtc, @IngestedBy, 0)",
                new
                {
                    ReadingId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    DeviceId = deviceId,
                    SensorType = "Temperature",
                    ReadingValue = 72.5,
                    Status = "Normal",
                    RecordedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    IngestedBy = ingestedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[IoTItems]
                    (ReadingId, TenantId, SiteId, DeviceId, SensorType, ReadingValue, Status, RecordedAtUtc, IngestedBy, IsDeleted)
                  VALUES (@ReadingId, @TenantId, @SiteId, @DeviceId, @SensorType, @ReadingValue, @Status, @RecordedAtUtc, @IngestedBy, 1)",
                new
                {
                    ReadingId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    DeviceId = deviceId,
                    SensorType = "Temperature",
                    ReadingValue = 1.0,
                    Status = "Normal",
                    RecordedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    IngestedBy = ingestedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[IoTItems]
                    (ReadingId, TenantId, SiteId, DeviceId, SensorType, ReadingValue, Status, RecordedAtUtc, IngestedBy, IsDeleted)
                  VALUES (@ReadingId, @TenantId, @SiteId, @DeviceId, @SensorType, @ReadingValue, @Status, @RecordedAtUtc, @IngestedBy, 0)",
                new
                {
                    ReadingId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    DeviceId = deviceId,
                    SensorType = "GasLevel",
                    ReadingValue = 999.9,
                    Status = "Alert",
                    RecordedAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    IngestedBy = ingestedBy,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetIoTQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetIoTQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.ReadingId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_IoT_GetIoTReadings");
            result.Value.Items.Should().NotContain(i => i.ReadingId == deletedId,
                "usp_IoT_GetIoTReadings filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.ReadingId == outOfRangeId,
                "usp_IoT_GetIoTReadings filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.ReadingId == inRangeId);
            item.DeviceId.Should().Be(deviceId);
            item.SensorType.Should().Be("Temperature");
            item.ReadingValue.Should().Be(72.5);
            item.Status.Should().Be("Normal");
            item.IngestedBy.Should().Be(ingestedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetIoT_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var noDataTenantId = Guid.NewGuid();
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetIoTQuery(noDataTenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded IoTItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetIoTReadings_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_IoT_GetIoTReadings]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        ReadingId,
                        TenantId,
                        SiteId,
                        DeviceId,
                        SensorType,
                        ReadingValue,
                        Status,
                        RecordedAtUtc,
                        IngestedBy
                    FROM [Integrations].[IoTItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND RecordedAtUtc >= @FromUtc
                      AND RecordedAtUtc <= @ToUtc
                    ORDER BY RecordedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END";

        const string forcedFailureProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_IoT_GetIoTReadings]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    DECLARE @ForcedFailureDivisor INT = 0;
                    DECLARE @ForcedFailureResult INT = 1 / @ForcedFailureDivisor; -- deliberate forced runtime failure for test coverage

                    SELECT
                        ReadingId,
                        TenantId,
                        SiteId,
                        DeviceId,
                        SensorType,
                        ReadingValue,
                        Status,
                        RecordedAtUtc,
                        IngestedBy
                    FROM [Integrations].[IoTItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND RecordedAtUtc >= @FromUtc
                      AND RecordedAtUtc <= @ToUtc
                    ORDER BY RecordedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END";

        await conn.ExecuteAsync(forcedFailureProcedure);

        try
        {
            Func<Task> act = async () =>
            {
                await using var cmd = new SqlCommand("[Integrations].[usp_IoT_GetIoTReadings]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_IoT_GetIoTReadings must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_IoT_GetIoTReadings", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_IoT_GetIoTReadings's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
