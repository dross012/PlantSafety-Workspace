// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Operational.BulkUpdate
//  Layer:          IntegrationTests
//  File:           BulkUpdateOperationalIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Exercises BulkUpdateOperationalCommand through the real MediatR pipeline and the
//                  real [Operational].[usp_OperationalResources_BulkUpdate] stored procedure.
//                  Regression test for a rowcount-concurrency bug: usp_OperationalResources_BulkUpdate
//                  uses SET NOCOUNT ON, so OperationalBulkDapperRepository.BulkUpdateAsync's original
//                  "rowsAffected > 0" check (against DapperCommandExecutor.ExecuteAsync, which always
//                  returns -1 under SET NOCOUNT ON) could never distinguish a real update from a
//                  miss -- every successful bulk update was silently reported as NOT_FOUND. The same
//                  bug was found and fixed in ComplianceBulkDapperRepository during the Compliance
//                  pass; fixed here by adding an explicit SELECT @@ROWCOUNT to the procedure and
//                  switching the repository to DapperCommandExecutor.ExecuteScalarAsync.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Operational.Commands;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Operational.BulkUpdate;

/// <summary>
/// Exercises <c>BulkUpdateOperationalCommand</c> through the real MediatR pipeline, the real
/// <c>OperationalBulkDapperRepository</c>, and the real <c>[Operational].[OperationalResources]</c>
/// table + <c>usp_OperationalResources_BulkUpdate</c> stored procedure.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class BulkUpdateOperationalIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task BulkUpdateOperational_Close_ShouldReportSuccessNotNotFound()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var resourceId = Guid.NewGuid();

            await using var conn = new SqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"INSERT INTO [Operational].[OperationalResources]
                    (Id, TenantId, SiteId, ResourceType, Status, UpdatedAtUtc)
                  VALUES
                    (@Id, @TenantId, NULL, N'Inspection', N'Open', @Now)",
                new { Id = resourceId, TenantId, Now = FixedUtcNow });

            var command = new BulkUpdateOperationalCommand(
                TenantId: TenantId,
                SiteId: null,
                ResourceIds: new[] { resourceId },
                Operation: "Close",
                Parameters: null);

            var result = await sender.Send(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Succeeded.Should().Be(1,
                "a resource that genuinely exists and was updated must report success, not NOT_FOUND " +
                "-- this is the regression case for the SET NOCOUNT ON rowcount bug");
            result.Value.Failed.Should().Be(0);
            result.Value.Results.Single().Success.Should().BeTrue();

            var statusAfter = await conn.ExecuteScalarAsync<string>(
                "SELECT Status FROM [Operational].[OperationalResources] WHERE Id = @Id", new { Id = resourceId });
            statusAfter.Should().Be("Closed");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task BulkUpdateOperational_ForcedNotNullViolation_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (NOT NULL constraint violation on UpdatedAtUtc) by
        // calling [Operational].[usp_OperationalResources_BulkUpdate] directly with @NowUtc = NULL
        // against a resource row that genuinely exists (so the UPDATE actually reaches the
        // NOT NULL column, rather than matching zero rows). Confirms the modernized procedure's
        // THROW still re-raises the exception and that the CATCH block's ErrorLog insert fires.
        var resourceId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync(
            @"INSERT INTO [Operational].[OperationalResources]
                (Id, TenantId, SiteId, ResourceType, Status, UpdatedAtUtc)
              VALUES
                (@Id, @TenantId, NULL, N'Inspection', N'Open', @Now)",
            new { Id = resourceId, TenantId, Now = FixedUtcNow });

        await using (var cmd = new SqlCommand("[Operational].[usp_OperationalResources_BulkUpdate]", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TenantId", TenantId);
            cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
            cmd.Parameters.AddWithValue("@ResourceId", resourceId);
            cmd.Parameters.AddWithValue("@Operation", "Close");
            cmd.Parameters.AddWithValue("@AssignedToUserId", DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", DBNull.Value);
            cmd.Parameters.AddWithValue("@NowUtc", DBNull.Value);

            var act = async () => await cmd.ExecuteScalarAsync();

            await act.Should().ThrowAsync<SqlException>(
                "a NULL @NowUtc must violate OperationalResources.UpdatedAtUtc's NOT NULL constraint and THROW must re-raise it");
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_OperationalResources_BulkUpdate", Pattern = $"%ResourceId={resourceId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_OperationalResources_BulkUpdate's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }

    [Fact]
    public async Task BulkUpdateOperational_UnknownResource_ShouldReportNotFound()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var command = new BulkUpdateOperationalCommand(
                TenantId: TenantId,
                SiteId: null,
                ResourceIds: new[] { Guid.NewGuid() },
                Operation: "Close",
                Parameters: null);

            var result = await sender.Send(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Failed.Should().Be(1);
            result.Value.Results.Single().Success.Should().BeFalse();
            result.Value.Results.Single().ErrorCode.Should().Be("NOT_FOUND");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
