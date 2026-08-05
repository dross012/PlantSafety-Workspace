// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Queries
//  Layer:          Tests (Unit)
//  File:           AdvancedMetricsQueryHandlersTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query handler tests for all six Sustainability.AdvancedMetrics read-model
//                  query handlers: GetCarbonFootprintByPeriod, GetEnergyIntensityTrend,
//                  GetWaterIntensityTrend, GetWasteDiversionRate, ListSustainabilityTargetsAndProgress,
//                  GetEsgDisclosureReportById. Covers the happy path, not-found mapping (where
//                  applicable), and the generic failure-envelope fallback when the read repository
//                  throws. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Queries;

public sealed class GetCarbonFootprintByPeriodQueryHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsReadRepository _repo = Substitute.For<IAdvancedMetricsReadRepository>();
    private GetCarbonFootprintByPeriodQueryHandler Sut() => new(_repo, NullLogger<GetCarbonFootprintByPeriodQueryHandler>());

    [Fact]
    public async Task Handle_WhenDataExists_ReturnsSuccess()
    {
        var ledgerId = Guid.NewGuid();
        _repo.GetCarbonFootprintByPeriodAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new CarbonFootprintByPeriodDto
            {
                AdvancedMetricsLedgerId = ledgerId,
                ReportingPeriodStartUtc = FixedUtcNow.AddMonths(-1),
                ReportingPeriodEndUtc = FixedUtcNow,
                TotalMetricTonsCo2e = 100m,
                RecordCount = 3,
            });

        var result = await Sut().Handle(new GetCarbonFootprintByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddMonths(-1), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AdvancedMetricsLedgerId.Should().Be(ledgerId);
    }

    [Fact]
    public async Task Handle_WhenNoDataExists_ReturnsNotFound()
    {
        _repo.GetCarbonFootprintByPeriodAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns((CarbonFootprintByPeriodDto?)null);

        var result = await Sut().Handle(new GetCarbonFootprintByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddMonths(-1), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetCarbonFootprintByPeriodAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<CarbonFootprintByPeriodDto?>>(_ => throw new InvalidOperationException("db down"));

        var result = await Sut().Handle(new GetCarbonFootprintByPeriodQuery(TestTenantId.Value, null, FixedUtcNow.AddMonths(-1), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.CarbonFootprint.RetrievalFailed");
    }
}

public sealed class GetEnergyIntensityTrendQueryHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsReadRepository _repo = Substitute.For<IAdvancedMetricsReadRepository>();
    private GetEnergyIntensityTrendQueryHandler Sut() => new(_repo, NullLogger<GetEnergyIntensityTrendQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsPointsFromRepository()
    {
        var ledgerId = Guid.NewGuid();
        IReadOnlyList<EnergyIntensityTrendPointDto> points =
        [
            new EnergyIntensityTrendPointDto { EnergyIntensityMetricId = Guid.NewGuid(), ReadingAtUtc = FixedUtcNow, EnergyConsumedKwh = 100m, OutputUnits = 40m, IntensityValue = 2.5m, MethodologyVersion = "v1" }
        ];
        _repo.GetEnergyIntensityTrendAsync(ledgerId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(points);

        var result = await Sut().Handle(new GetEnergyIntensityTrendQuery(TestTenantId.Value, ledgerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(points);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetEnergyIntensityTrendAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<EnergyIntensityTrendPointDto>>>(_ => throw new InvalidOperationException("db down"));

        var result = await Sut().Handle(new GetEnergyIntensityTrendQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.EnergyIntensityTrend.RetrievalFailed");
    }
}

public sealed class GetWaterIntensityTrendQueryHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsReadRepository _repo = Substitute.For<IAdvancedMetricsReadRepository>();
    private GetWaterIntensityTrendQueryHandler Sut() => new(_repo, NullLogger<GetWaterIntensityTrendQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsPointsFromRepository()
    {
        var ledgerId = Guid.NewGuid();
        IReadOnlyList<WaterIntensityTrendPointDto> points =
        [
            new WaterIntensityTrendPointDto { WaterIntensityMetricId = Guid.NewGuid(), ReadingAtUtc = FixedUtcNow, WaterConsumedCubicMeters = 300m, OutputUnits = 500m, IntensityValue = 0.6m, MethodologyVersion = "v1" }
        ];
        _repo.GetWaterIntensityTrendAsync(ledgerId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(points);

        var result = await Sut().Handle(new GetWaterIntensityTrendQuery(TestTenantId.Value, ledgerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(points);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetWaterIntensityTrendAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<WaterIntensityTrendPointDto>>>(_ => throw new InvalidOperationException("db down"));

        var result = await Sut().Handle(new GetWaterIntensityTrendQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.WaterIntensityTrend.RetrievalFailed");
    }
}

public sealed class GetWasteDiversionRateQueryHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsReadRepository _repo = Substitute.For<IAdvancedMetricsReadRepository>();
    private GetWasteDiversionRateQueryHandler Sut() => new(_repo, NullLogger<GetWasteDiversionRateQueryHandler>());

    [Fact]
    public async Task Handle_WhenDataExists_ReturnsSuccess()
    {
        var ledgerId = Guid.NewGuid();
        _repo.GetWasteDiversionRateAsync(ledgerId, TestTenantId.Value, Arg.Any<CancellationToken>())
            .Returns(new WasteDiversionRateDto { AdvancedMetricsLedgerId = ledgerId, TotalWasteKg = 1000m, DivertedWasteKg = 750m, DiversionRatePercent = 75m, RecordCount = 1 });

        var result = await Sut().Handle(new GetWasteDiversionRateQuery(TestTenantId.Value, ledgerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DiversionRatePercent.Should().Be(75m);
    }

    [Fact]
    public async Task Handle_WhenNoDataExists_ReturnsNotFound()
    {
        _repo.GetWasteDiversionRateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WasteDiversionRateDto?)null);

        var result = await Sut().Handle(new GetWasteDiversionRateQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }
}

public sealed class ListSustainabilityTargetsAndProgressQueryHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsReadRepository _repo = Substitute.For<IAdvancedMetricsReadRepository>();
    private ListSustainabilityTargetsAndProgressQueryHandler Sut() => new(_repo, NullLogger<ListSustainabilityTargetsAndProgressQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsTargetsFromRepository()
    {
        var ledgerId = Guid.NewGuid();
        IReadOnlyList<SustainabilityTargetProgressDto> targets =
        [
            new SustainabilityTargetProgressDto
            {
                SustainabilityTargetId = Guid.NewGuid(), AdvancedMetricsLedgerId = ledgerId, TargetType = "WasteDiversionRate",
                BaselineValue = 40m, TargetValue = 75m, CurrentValue = 50m, TargetDateUtc = FixedUtcNow.AddYears(1), Status = "OnTrack",
            }
        ];
        _repo.ListSustainabilityTargetsAndProgressAsync(ledgerId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(targets);

        var result = await Sut().Handle(new ListSustainabilityTargetsAndProgressQuery(TestTenantId.Value, ledgerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(targets);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.ListSustainabilityTargetsAndProgressAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<SustainabilityTargetProgressDto>>>(_ => throw new InvalidOperationException("db down"));

        var result = await Sut().Handle(new ListSustainabilityTargetsAndProgressQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.SustainabilityTargets.RetrievalFailed");
    }
}

public sealed class GetEsgDisclosureReportByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsReadRepository _repo = Substitute.For<IAdvancedMetricsReadRepository>();
    private GetEsgDisclosureReportByIdQueryHandler Sut() => new(_repo, NullLogger<GetEsgDisclosureReportByIdQueryHandler>());

    private static EsgDisclosureReportDto ReportFor(Guid reportId, Guid tenantId) => new()
    {
        EsgDisclosureReportId = reportId, TenantId = tenantId, SourceLedgerId = Guid.NewGuid(), Frameworks = "GhgProtocol",
        ReportingPeriodStartUtc = FixedUtcNow.AddMonths(-3), ReportingPeriodEndUtc = FixedUtcNow, Version = 1,
        MethodologyVersion = "v1", Status = "Draft", GeneratedAtUtc = FixedUtcNow, GeneratedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenReportExistsForTenant_ReturnsSuccess()
    {
        var reportId = Guid.NewGuid();
        _repo.GetEsgDisclosureReportByIdAsync(reportId, TestTenantId.Value, Arg.Any<CancellationToken>())
            .Returns(ReportFor(reportId, TestTenantId.Value));

        var result = await Sut().Handle(new GetEsgDisclosureReportByIdQuery(TestTenantId.Value, reportId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EsgDisclosureReportId.Should().Be(reportId);
    }

    [Fact]
    public async Task Handle_WhenReportDoesNotExist_ReturnsNotFound()
    {
        _repo.GetEsgDisclosureReportByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EsgDisclosureReportDto?)null);

        var result = await Sut().Handle(new GetEsgDisclosureReportByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenReportBelongsToDifferentTenant_ReturnsNotFoundNotTheOtherTenantsData()
    {
        var reportId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        // Simulates a permissive repository implementation returning a cross-tenant row; the
        // handler itself must be the final guard that refuses to leak it.
        _repo.GetEsgDisclosureReportByIdAsync(reportId, TestTenantId.Value, Arg.Any<CancellationToken>())
            .Returns(ReportFor(reportId, otherTenantId));

        var result = await Sut().Handle(new GetEsgDisclosureReportByIdQuery(TestTenantId.Value, reportId), CancellationToken.None);

        result.IsFailure.Should().BeTrue("a report belonging to a different tenant must never be revealed to the caller");
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.EsgDisclosureReport.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }
}
