// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Handlers
//  Layer:          Tests (Unit)
//  File:           WaterDischargeComplianceQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query handler tests for the 6 WaterDischargeCompliance read-model queries:
//                  ListDischargeLimitsByPointQuery, GetDischargeMonitoringReportsByPeriodQuery,
//                  GetDischargePermitByIdQuery, ListDischargePointsBySiteQuery,
//                  GetSampleHistoryByPointQuery, ListActiveExceedancesQuery.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Handlers;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Repositories;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Handlers;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Repositories;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Handlers;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Repositories;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Handlers;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Repositories;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Handlers;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Repositories;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Handlers;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Repositories;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Handlers;

public sealed class WaterDischargeComplianceQueryHandlerTests : UnitTestBase
{
    [Fact]
    public async Task ListDischargeLimitsByPoint_ReturnsItemsFromRepository()
    {
        var repo = Substitute.For<IDischargeLimitRepository>();
        var pointId = Guid.NewGuid();
        var items = new List<DischargeLimitItemDto>
        {
            new()
            {
                DischargeLimitId = Guid.NewGuid(), TenantId = TestTenantId.Value, DischargePointId = pointId,
                Parameter = "PH", LimitBasis = "DailyMaximum", LimitValue = 9.0m, UnitOfMeasure = "pH",
                PermitConditionReference = "Part I.A.1", Status = "Active", CreatedAtUtc = FixedUtcNow, CreatedBy = TestUserId.Value,
            },
        };
        repo.ListDischargeLimitsByPointAsync(TestTenantId.Value, pointId, Arg.Any<CancellationToken>()).Returns(items);
        var sut = new ListDischargeLimitsByPointQueryHandler(repo, NullLogger<ListDischargeLimitsByPointQueryHandler>());

        var result = await sut.Handle(new ListDischargeLimitsByPointQuery(TestTenantId.Value, pointId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetDischargeMonitoringReportsByPeriod_ReturnsItemsFromRepository()
    {
        var repo = Substitute.For<IDischargeMonitoringReportRepository>();
        var items = new List<DischargeMonitoringReportItemDto>
        {
            new()
            {
                DischargeMonitoringReportId = Guid.NewGuid(), TenantId = TestTenantId.Value, DischargePermitId = Guid.NewGuid(),
                ReportingPeriodStartUtc = FixedUtcNow.AddMonths(-1), ReportingPeriodEndUtc = FixedUtcNow, DueAtUtc = FixedUtcNow.AddDays(28),
                SamplesWithinLimitCount = 28, TotalSamplesCount = 30, Status = "Draft", CreatedAtUtc = FixedUtcNow, CreatedBy = TestUserId.Value,
            },
        };
        repo.GetDischargeMonitoringReportsByPeriodAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(items);
        var sut = new GetDischargeMonitoringReportsByPeriodQueryHandler(repo, NullLogger<GetDischargeMonitoringReportsByPeriodQueryHandler>());

        var result = await sut.Handle(
            new GetDischargeMonitoringReportsByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddMonths(-1), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetDischargePermitById_WhenFound_ReturnsPermitDto()
    {
        var repo = Substitute.For<IDischargePermitRepository>();
        var permitId = Guid.NewGuid();
        var dto = new DischargePermitItemDto
        {
            DischargePermitId = permitId, TenantId = TestTenantId.Value, PermitNumber = "NPDES-1",
            RegulatoryProgram = "Npdes", IssuingAuthority = "State EPA", EffectiveAtUtc = FixedUtcNow,
            ExpiresAtUtc = FixedUtcNow.AddYears(1), RenewalCount = 0, Status = "Active",
            CreatedAtUtc = FixedUtcNow, CreatedBy = TestUserId.Value,
        };
        repo.GetDischargePermitByIdAsync(TestTenantId.Value, permitId, Arg.Any<CancellationToken>()).Returns(dto);
        var sut = new GetDischargePermitByIdQueryHandler(repo, NullLogger<GetDischargePermitByIdQueryHandler>());

        var result = await sut.Handle(new GetDischargePermitByIdQuery(TestTenantId.Value, permitId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetDischargePermitById_WhenNotFound_ReturnsSuccessWithNullItem()
    {
        var repo = Substitute.For<IDischargePermitRepository>();
        repo.GetDischargePermitByIdAsync(TestTenantId.Value, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DischargePermitItemDto?)null);
        var sut = new GetDischargePermitByIdQueryHandler(repo, NullLogger<GetDischargePermitByIdQueryHandler>());

        var result = await sut.Handle(new GetDischargePermitByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeNull();
    }

    [Fact]
    public async Task ListDischargePointsBySite_ReturnsItemsFromRepository()
    {
        var repo = Substitute.For<IDischargePointRepository>();
        var items = new List<DischargePointItemDto>
        {
            new()
            {
                DischargePointId = Guid.NewGuid(), TenantId = TestTenantId.Value, DischargePermitId = Guid.NewGuid(),
                PointCode = "Outfall 001", PointName = "Main Outfall", ReceivingWaterBody = "Willow Creek",
                Outfall = "Direct", Status = "Active", CreatedAtUtc = FixedUtcNow, CreatedBy = TestUserId.Value,
            },
        };
        repo.ListDischargePointsBySiteAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);
        var sut = new ListDischargePointsBySiteQueryHandler(repo, NullLogger<ListDischargePointsBySiteQueryHandler>());

        var result = await sut.Handle(new ListDischargePointsBySiteQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetSampleHistoryByPoint_ReturnsItemsFromRepository()
    {
        var repo = Substitute.For<IEffluentSampleRepository>();
        var pointId = Guid.NewGuid();
        var items = new List<EffluentSampleItemDto>
        {
            new()
            {
                EffluentSampleId = Guid.NewGuid(), TenantId = TestTenantId.Value, DischargePointId = pointId,
                Parameter = "PH", MeasuredValue = 7.2m, UnitOfMeasure = "pH", SampledAtUtc = FixedUtcNow,
                LabSourceSystem = "LIMS-Prod", ChainOfCustodyReference = "COC-0001", Status = "Recorded",
                CreatedAtUtc = FixedUtcNow, CreatedBy = TestUserId.Value,
            },
        };
        repo.GetSampleHistoryByPointAsync(TestTenantId.Value, pointId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(items);
        var sut = new GetSampleHistoryByPointQueryHandler(repo, NullLogger<GetSampleHistoryByPointQueryHandler>());

        var result = await sut.Handle(
            new GetSampleHistoryByPointQuery(TestTenantId.Value, pointId, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task ListActiveExceedances_ReturnsItemsFromRepository()
    {
        var repo = Substitute.For<ILimitExceedanceRepository>();
        var items = new List<LimitExceedanceItemDto>
        {
            new()
            {
                LimitExceedanceId = Guid.NewGuid(), TenantId = TestTenantId.Value, DischargePointId = Guid.NewGuid(),
                ParameterName = "pH", MeasuredValue = 10.5m, LimitValue = 9.0m, ExceedancePercentage = 16.67m,
                Severity = "Major", DetectedAtUtc = FixedUtcNow, Status = "Detected", CreatedAtUtc = FixedUtcNow, CreatedBy = TestUserId.Value,
            },
        };
        repo.ListActiveExceedancesAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);
        var sut = new ListActiveExceedancesQueryHandler(repo, NullLogger<ListActiveExceedancesQueryHandler>());

        var result = await sut.Handle(new ListActiveExceedancesQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task ListActiveExceedances_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        var repo = Substitute.For<ILimitExceedanceRepository>();
        repo.ListActiveExceedancesAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<LimitExceedanceItemDto>>>(_ => throw new InvalidOperationException("transient failure"));
        var sut = new ListActiveExceedancesQueryHandler(repo, NullLogger<ListActiveExceedancesQueryHandler>());

        var result = await sut.Handle(new ListActiveExceedancesQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
