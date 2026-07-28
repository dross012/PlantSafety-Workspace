// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Compliance.BulkUpdate
//  Layer:          IntegrationTests
//  File:           BulkUpdateComplianceIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Exercises BulkUpdateComplianceCommand through the real MediatR pipeline and the
//                  real [Compliance].[usp_ComplianceResources_BulkUpdate] stored procedure.
//                  Regression test for a rowcount-concurrency bug: usp_ComplianceResources_BulkUpdate
//                  uses SET NOCOUNT ON, so ComplianceBulkDapperRepository.BulkUpdateAsync's original
//                  "rowsAffected > 0" check (against DapperCommandExecutor.ExecuteAsync, which always
//                  returns -1 under SET NOCOUNT ON) could never distinguish a real update from a
//                  miss -- every successful bulk update was silently reported as NOT_FOUND. Fixed by
//                  adding an explicit SELECT @@ROWCOUNT to the procedure and switching the repository
//                  to DapperCommandExecutor.ExecuteScalarAsync.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Compliance.Commands;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Compliance.BulkUpdate;

/// <summary>
/// Exercises <c>BulkUpdateComplianceCommand</c> through the real MediatR pipeline, the real
/// <c>ComplianceBulkDapperRepository</c>, and the real <c>[Compliance].[ComplianceResources]</c>
/// table + <c>usp_ComplianceResources_BulkUpdate</c> stored procedure.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class BulkUpdateComplianceIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task BulkUpdateCompliance_Close_ShouldReportSuccessNotNotFound()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var resourceId = Guid.NewGuid();

            await using var conn = new SqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"INSERT INTO [Compliance].[ComplianceResources]
                    (Id, TenantId, SiteId, ResourceType, Status, UpdatedAtUtc)
                  VALUES
                    (@Id, @TenantId, NULL, N'Obligation', N'Open', @Now)",
                new { Id = resourceId, TenantId, Now = FixedUtcNow });

            var command = new BulkUpdateComplianceCommand(
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
                "SELECT Status FROM [Compliance].[ComplianceResources] WHERE Id = @Id", new { Id = resourceId });
            statusAfter.Should().Be("Closed");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task BulkUpdateCompliance_ForcedNotNullViolation_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (NOT NULL constraint violation on UpdatedAtUtc) by
        // calling [Compliance].[usp_ComplianceResources_BulkUpdate] directly with @NowUtc = NULL
        // against a resource row that genuinely exists (so the UPDATE actually reaches the
        // NOT NULL column, rather than matching zero rows). Confirms the modernized procedure's
        // THROW still re-raises the exception and that the CATCH block's ErrorLog insert fires.
        var resourceId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync(
            @"INSERT INTO [Compliance].[ComplianceResources]
                (Id, TenantId, SiteId, ResourceType, Status, UpdatedAtUtc)
              VALUES
                (@Id, @TenantId, NULL, N'Obligation', N'Open', @Now)",
            new { Id = resourceId, TenantId, Now = FixedUtcNow });

        await using (var cmd = new SqlCommand("[Compliance].[usp_ComplianceResources_BulkUpdate]", conn))
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
                "a NULL @NowUtc must violate ComplianceResources.UpdatedAtUtc's NOT NULL constraint and THROW must re-raise it");
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_ComplianceResources_BulkUpdate", Pattern = $"%ResourceId={resourceId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_ComplianceResources_BulkUpdate's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }

    [Fact]
    public async Task BulkUpdateCompliance_UnknownResource_ShouldReportNotFound()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var command = new BulkUpdateComplianceCommand(
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
