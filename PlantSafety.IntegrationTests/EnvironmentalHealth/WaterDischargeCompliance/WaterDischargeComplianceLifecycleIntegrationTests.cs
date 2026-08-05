// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.EnvironmentalHealth.WaterDischargeCompliance
//  Layer:          IntegrationTests
//  File:           WaterDischargeComplianceLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Full command/query round-trip coverage for all 6 WaterDischargeCompliance
//                  aggregates (DischargePermit, DischargePoint, EffluentSample, DischargeLimit,
//                  LimitExceedance, DischargeMonitoringReport) through the real MediatR pipeline,
//                  the real Dapper repositories, and the real [EnvironmentalHealth].* tables +
//                  stored procedures applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyWaterDischargeComplianceSchemaAsync). Includes a
//                  regression test proving InitiateExceedanceResponseCommandHandler and
//                  CloseExceedanceResponseCommandHandler work end-to-end with a plain Guid
//                  ResponseUserId round-tripped through the real database (both handlers previously
//                  had a Guid-vs-UserId type-mismatch bug, hand-fixed via UserId.FromGuid(...)).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.EnvironmentalHealth.WaterDischargeCompliance;

[Collection(nameof(IntegrationTestCollection))]
public sealed class WaterDischargeComplianceLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task DischargePermitLifecycle_IssueRenewSuspend_ShouldRoundTripThroughRealDatabase()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var issueResult = await sender.Send(new IssueDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"NPDES-{Guid.NewGuid():N}", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1),
                "Standard NPDES conditions.", ActorId), CancellationToken.None);

            issueResult.IsSuccess.Should().BeTrue();
            var permitId = issueResult.Value.DischargePermitId;

            var getAfterIssue = await sender.Send(new GetDischargePermitByIdQuery(TenantId, permitId), CancellationToken.None);
            getAfterIssue.IsSuccess.Should().BeTrue();
            getAfterIssue.Value.Items.Should().NotBeNull();
            getAfterIssue.Value.Items!.Status.Should().Be("Active");

            var renewResult = await sender.Send(new RenewDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, permitId, FixedUtcNow.AddYears(2), ActorId), CancellationToken.None);
            renewResult.IsSuccess.Should().BeTrue();
            renewResult.Value.Status.Should().Be("Active");

            var suspendResult = await sender.Send(new SuspendDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, permitId, "Effluent violation under investigation.", ActorId), CancellationToken.None);
            suspendResult.IsSuccess.Should().BeTrue();
            suspendResult.Value.Status.Should().Be("Suspended");

            var getAfterSuspend = await sender.Send(new GetDischargePermitByIdQuery(TenantId, permitId), CancellationToken.None);
            getAfterSuspend.Value.Items!.Status.Should().Be("Suspended");
            getAfterSuspend.Value.Items.RenewalCount.Should().Be(1);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task DischargePointLifecycle_RegisterDeactivateReactivate_ShouldRoundTripThroughRealDatabase()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var permitId = (await sender.Send(new IssueDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"NPDES-{Guid.NewGuid():N}", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1),
                "conditions", ActorId), CancellationToken.None)).Value.DischargePermitId;

            var registerResult = await sender.Send(new RegisterDischargePointCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                permitId, "Outfall 001", "Main Outfall", "Willow Creek", "Direct", null, null, ActorId), CancellationToken.None);
            registerResult.IsSuccess.Should().BeTrue();
            var pointId = registerResult.Value.DischargePointId;

            var deactivateResult = await sender.Send(new DeactivateDischargePointCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, pointId, "Seasonal shutdown.", ActorId), CancellationToken.None);
            deactivateResult.IsSuccess.Should().BeTrue();
            deactivateResult.Value.Status.Should().Be("Inactive");

            var reactivateResult = await sender.Send(new ReactivateDischargePointCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, pointId, ActorId), CancellationToken.None);
            reactivateResult.IsSuccess.Should().BeTrue();
            reactivateResult.Value.Status.Should().Be("Active");

            var listResult = await sender.Send(new ListDischargePointsBySiteQuery(TenantId, null), CancellationToken.None);
            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(p => p.DischargePointId == pointId && p.Status == "Active");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task EffluentSampleAndDischargeLimitLifecycle_ShouldRoundTripThroughRealDatabase()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var permitId = (await sender.Send(new IssueDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"NPDES-{Guid.NewGuid():N}", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1),
                "conditions", ActorId), CancellationToken.None)).Value.DischargePermitId;
            var pointId = (await sender.Send(new RegisterDischargePointCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                permitId, "Outfall 002", "Secondary Outfall", "Willow Creek", "Direct", null, null, ActorId), CancellationToken.None)).Value.DischargePointId;

            var sampleResult = await sender.Send(new RecordEffluentSampleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, pointId,
                "PH", 7.2m, "pH", FixedUtcNow, ActorId, "LIMS-Prod", $"COC-{Guid.NewGuid():N}", null, null, ActorId), CancellationToken.None);
            sampleResult.IsSuccess.Should().BeTrue();
            sampleResult.Value.Status.Should().Be("Recorded");

            var voidResult = await sender.Send(new VoidEffluentSampleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, sampleResult.Value.EffluentSampleId, "Chain of custody broken.", ActorId), CancellationToken.None);
            voidResult.IsSuccess.Should().BeTrue();
            voidResult.Value.Status.Should().Be("Voided");

            var defineLimitResult = await sender.Send(new DefineDischargeLimitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, pointId, permitId,
                "PH", "DailyMaximum", 9.0m, "pH", FixedUtcNow, "Part I.A.1", ActorId), CancellationToken.None);
            defineLimitResult.IsSuccess.Should().BeTrue();
            var limitId = defineLimitResult.Value.DischargeLimitId;

            var reviseResult = await sender.Send(new ReviseDischargeLimitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, limitId, 8.5m, ActorId), CancellationToken.None);
            reviseResult.IsSuccess.Should().BeTrue();

            var retireResult = await sender.Send(new RetireDischargeLimitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, limitId, "Superseded.", ActorId), CancellationToken.None);
            retireResult.IsSuccess.Should().BeTrue();
            retireResult.Value.Status.Should().Be("Retired");

            var listLimits = await sender.Send(new ListDischargeLimitsByPointQuery(TenantId, pointId), CancellationToken.None);
            listLimits.Value.Items.Should().Contain(l => l.DischargeLimitId == limitId && l.Status == "Retired" && l.LimitValue == 8.5m);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task DischargeMonitoringReportLifecycle_PrepareSubmitAccept_ShouldRoundTripThroughRealDatabase()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var permitId = (await sender.Send(new IssueDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"NPDES-{Guid.NewGuid():N}", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1),
                "conditions", ActorId), CancellationToken.None)).Value.DischargePermitId;

            var periodStart = FixedUtcNow.AddMonths(-1);
            var periodEnd = FixedUtcNow;
            var prepareResult = await sender.Send(new PrepareDischargeMonitoringReportCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, permitId,
                periodStart, periodEnd, periodEnd.AddDays(28), ActorId), CancellationToken.None);
            prepareResult.IsSuccess.Should().BeTrue();
            var reportId = prepareResult.Value.DischargeMonitoringReportId;

            var submitResult = await sender.Send(new SubmitDischargeMonitoringReportCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, reportId, periodEnd.AddDays(1), 28, 30, ActorId), CancellationToken.None);
            submitResult.IsSuccess.Should().BeTrue();
            submitResult.Value.Status.Should().Be("Submitted");

            var acceptResult = await sender.Send(new AcceptDischargeMonitoringReportCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, reportId, ActorId), CancellationToken.None);
            acceptResult.IsSuccess.Should().BeTrue();
            acceptResult.Value.Status.Should().Be("Accepted");

            var listResult = await sender.Send(new GetDischargeMonitoringReportsByPeriodQuery(TenantId, null, periodStart, periodEnd), CancellationToken.None);
            listResult.Value.Items.Should().Contain(r => r.DischargeMonitoringReportId == reportId && r.Status == "Accepted");
        }
        finally
        {
            scope.Dispose();
        }
    }

    // ---- Regression: Guid-vs-UserId type-mismatch fix, exercised end-to-end against the real DB ----

    [Fact]
    public async Task LimitExceedanceResponseWorkflow_WithPlainGuidResponseUserId_ShouldRoundTripThroughRealDatabase()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var permitId = (await sender.Send(new IssueDischargePermitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"NPDES-{Guid.NewGuid():N}", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1),
                "conditions", ActorId), CancellationToken.None)).Value.DischargePermitId;
            var pointId = (await sender.Send(new RegisterDischargePointCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                permitId, "Outfall 003", "Third Outfall", "Willow Creek", "Direct", null, null, ActorId), CancellationToken.None)).Value.DischargePointId;
            var sampleId = (await sender.Send(new RecordEffluentSampleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, pointId,
                "PH", 10.5m, "pH", FixedUtcNow, ActorId, "LIMS-Prod", $"COC-{Guid.NewGuid():N}", null, null, ActorId), CancellationToken.None)).Value.EffluentSampleId;
            var limitId = (await sender.Send(new DefineDischargeLimitCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, pointId, permitId,
                "PH", "DailyMaximum", 9.0m, "pH", FixedUtcNow, "Part I.A.1", ActorId), CancellationToken.None)).Value.DischargeLimitId;

            var recordResult = await sender.Send(new RecordLimitExceedanceCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, pointId, sampleId, limitId,
                "pH", 10.5m, 9.0m, "Major", FixedUtcNow, 0, ActorId), CancellationToken.None);
            recordResult.IsSuccess.Should().BeTrue();
            var exceedanceId = recordResult.Value.LimitExceedanceId;

            // ResponseUserId arrives as a plain Guid from the request contract, exactly as it did
            // when the type-mismatch bug was present -- this must succeed end-to-end against the
            // real database, not just against a fake/mocked repository.
            var responseUserId = Guid.NewGuid();
            var initiateResult = await sender.Send(new InitiateExceedanceResponseCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, exceedanceId, responseUserId,
                "Investigating root cause.", ActorId), CancellationToken.None);
            initiateResult.IsSuccess.Should().BeTrue("InitiateExceedanceResponseCommandHandler must accept a plain Guid ResponseUserId end-to-end");
            initiateResult.Value.Status.Should().Be("ResponseInitiated");

            var closeResult = await sender.Send(new CloseExceedanceResponseCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, exceedanceId, responseUserId,
                "Root cause corrected; resampled within limit.", ActorId), CancellationToken.None);
            closeResult.IsSuccess.Should().BeTrue("CloseExceedanceResponseCommandHandler must accept a plain Guid ResponseUserId end-to-end");
            closeResult.Value.Status.Should().Be("Closed");

            var activeList = await sender.Send(new ListActiveExceedancesQuery(TenantId, null), CancellationToken.None);
            activeList.Value.Items.Should().NotContain(e => e.LimitExceedanceId == exceedanceId,
                "a Closed exceedance must no longer appear in the active-exceedances read model");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task InitiateExceedanceResponse_WhenExceedanceDoesNotExist_ShouldReturnNotFoundFailure()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new InitiateExceedanceResponseCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), Guid.NewGuid(), null, ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Contain("NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
