// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.CommunitySafety
//  Layer:          IntegrationTests
//  File:           CommunitySafetySqlTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the CommunitySafety module,
//                  exercising the real MediatR pipeline (command/query handlers) through the real
//                  DI container against the Dapper repositories and stored procedures applied by
//                  TestDatabaseFactory.ApplyCommunitySafetySchemaAsync. Covers all five aggregates:
//                  CommunityContact, CommunityConcern, CommunityImpactAssessment,
//                  CommunityNotification, CommunityMeetingRecord. Deterministic, parallel-safe,
//                  additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;
using PlantSafety.Application.CommunitySafety.CommunityContact.Queries;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Queries;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Queries;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.CommunitySafety.Sql;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CommunitySafetySqlTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Schema_ShouldExist_InDatabase()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.schemas WHERE name = 'CommunitySafety'");

        count.Should().Be(1, "the CommunitySafety schema must exist before running its SQL tests");
    }

    [Theory]
    [InlineData("CommunityContact")]
    [InlineData("CommunityConcern")]
    [InlineData("CommunityImpactAssessment")]
    [InlineData("CommunityNotification")]
    [InlineData("CommunityMeetingRecord")]
    public async Task Table_ShouldExist_InCommunitySafetySchema(string tableName)
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.tables WHERE name = @tableName AND schema_id = SCHEMA_ID('CommunitySafety')",
            new { tableName });

        count.Should().Be(1, $"[CommunitySafety].[{tableName}] must exist");
    }

    [Fact]
    public async Task RegisterCommunityContact_ShouldRoundTrip_AndSurfaceInQuery()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(
                new RegisterCommunityContactCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                    "Fire Chief - Station 4", "EmergencyServices", "North Ridge", "chief@example.org",
                    "City Fire Dept", "555-0100", true, ActorId),
                CancellationToken.None);

            registerResult.IsSuccess.Should().BeTrue("RegisterCommunityContactCommand must succeed against the real stored procedure");
            registerResult.Value.Status.Should().Be("Active");

            var getResult = await sender.Send(
                new GetCommunityContactsQuery(TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)),
                CancellationToken.None);

            getResult.IsSuccess.Should().BeTrue();
            getResult.Value.Items.Should().Contain(i => i.CommunityContactId == registerResult.Value.CommunityContactId && i.DisplayName == "Fire Chief - Station 4");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task LogCommunityConcern_ThenResolve_ShouldPersistLifecycleAcrossRoundTrips()
    {
        var siteId = Guid.NewGuid();
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var logResult = await sender.Send(
                new LogCommunityConcernCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, siteId,
                    "Odor", "Medium", "Tank Farm", "Strong odor reported near the fence line.", null, null, ActorId),
                CancellationToken.None);

            logResult.IsSuccess.Should().BeTrue();
            logResult.Value.Status.Should().Be("Open");
            var concernId = logResult.Value.CommunityConcernId;

            var resolveResult = await sender.Send(
                new ResolveCommunityConcernCommand(Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, concernId, "Root cause corrected.", ActorId),
                CancellationToken.None);

            resolveResult.IsSuccess.Should().BeTrue();
            resolveResult.Value.Status.Should().Be("Resolved");

            var getByIdResult = await sender.Send(new GetCommunityConcernByIdQuery(TenantId, concernId), CancellationToken.None);
            getByIdResult.IsSuccess.Should().BeTrue();
            getByIdResult.Value.ConcernStatus.Should().Be("Resolved");
            getByIdResult.Value.ResolvedAtUtc.Should().NotBeNull();

            var openListResult = await sender.Send(new ListOpenCommunityConcernsQuery(TenantId, siteId), CancellationToken.None);
            openListResult.IsSuccess.Should().BeTrue();
            openListResult.Value.Items.Should().NotContain(i => i.CommunityConcernId == concernId, "a Resolved concern must not appear in the open-concerns list");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ResolveCommunityConcern_WhenConcernDoesNotExist_ShouldReturnNotFound()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(
                new ResolveCommunityConcernCommand(Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), "notes", ActorId),
                CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("CommunitySafety.CommunityConcern.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordCommunityImpactAssessment_ShouldRoundTrip_AndSurfaceInIncidentQuery()
    {
        var incidentId = Guid.NewGuid();
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var recordResult = await sender.Send(
                new RecordCommunityImpactAssessmentCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), incidentId,
                    "Severe", "North Ridge", "Vapor cloud drifted toward residential area.", 25, ActorId, ActorId),
                CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Recorded");

            var byIncidentResult = await sender.Send(new GetImpactAssessmentsByIncidentQuery(TenantId, incidentId), CancellationToken.None);

            byIncidentResult.IsSuccess.Should().BeTrue();
            byIncidentResult.Value.Items.Should().ContainSingle(i =>
                i.CommunityImpactAssessmentId == recordResult.Value.CommunityImpactAssessmentId
                && i.RequiresRightToKnowReporting);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task SendCommunityNotification_ShouldRoundTrip_AndSurfaceInPeriodQuery()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var sendResult = await sender.Send(
                new SendCommunityNotificationCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, "Email",
                    Guid.NewGuid(), "Planned maintenance notice", "Elevated flare activity expected Tuesday 2-4am.",
                    null, null, ActorId),
                CancellationToken.None);

            sendResult.IsSuccess.Should().BeTrue();
            sendResult.Value.Status.Should().Be("Sent");

            var listResult = await sender.Send(
                new ListNotificationsByPeriodQuery(TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)),
                CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(i => i.CommunityNotificationId == sendResult.Value.CommunityNotificationId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordCommunityMeeting_ShouldRoundTrip_AndSurfaceInPeriodQuery()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var recordResult = await sender.Send(
                new RecordCommunityMeetingCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(),
                    "Q3 Community Advisory Panel", FixedUtcNow, "Community Center, Room B", 12,
                    "Reviewed Q3 flare activity.", "Publish schedule.", ActorId, ActorId),
                CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Recorded");

            var listResult = await sender.Send(
                new GetCommunityMeetingRecordsQuery(TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)),
                CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(i => i.CommunityMeetingRecordId == recordResult.Value.CommunityMeetingRecordId);
        }
        finally
        {
            scope.Dispose();
        }
    }
}
