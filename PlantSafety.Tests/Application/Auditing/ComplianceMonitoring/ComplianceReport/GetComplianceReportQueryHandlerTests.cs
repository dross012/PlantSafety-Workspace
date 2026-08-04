// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Tests for GetComplianceReportQueryHandler -- confirms the compliance report
//   genuinely composes all three real data sources (TRIR/LTIR, audit-trail/incident lineage,
//   cross-site actions) via the SharedKernel IReportSectionProvider<,> cross-module composition
//   contract, and that a single section's failure degrades only that section rather than failing
//   the whole report.
// SchemaVersion: 1

using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;
using PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Handlers;
using PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Queries;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Contracts;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;
using PlantSafety.Application.GlobalSafetyInsights.Contracts;
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Tests.Application.Auditing.ComplianceMonitoring.ComplianceReport;

/// <summary>Tests for <see cref="GetComplianceReportQueryHandler"/>.</summary>
public sealed class GetComplianceReportQueryHandlerTests : UnitTestBase
{
    private readonly IReportSectionProvider<GlobalSafetyKpiSectionRequest, GlobalSafetyInsightsItemDto?> _safetyKpiProvider =
        Substitute.For<IReportSectionProvider<GlobalSafetyKpiSectionRequest, GlobalSafetyInsightsItemDto?>>();

    private readonly IReportSectionProvider<TenantAuditStreamSectionRequest, TenantAuditStreamResult> _auditTrailProvider =
        Substitute.For<IReportSectionProvider<TenantAuditStreamSectionRequest, TenantAuditStreamResult>>();

    private readonly IReportSectionProvider<CrossSiteActionLogSectionRequest, IReadOnlyList<CrossSiteActionLogEntryDto>> _crossSiteActionsProvider =
        Substitute.For<IReportSectionProvider<CrossSiteActionLogSectionRequest, IReadOnlyList<CrossSiteActionLogEntryDto>>>();

    private GetComplianceReportQueryHandler CreateSut() =>
        new(_safetyKpiProvider, _auditTrailProvider, _crossSiteActionsProvider, NullLogger<GetComplianceReportQueryHandler>());

    private static GlobalSafetyInsightsItemDto CreateKpiSnapshot() => new()
    {
        GlobalSafetyInsightsId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        RegionCode = "UsEast",
        SiteIds = [Guid.NewGuid()],
        WindowStartUtc = FixedUtcNow.AddDays(-30),
        WindowEndUtc = FixedUtcNow,
        Trir = 2.5m,
        Ltir = 1.1m,
        ExposureRate = 10m,
        IncidentDensity = 0.2m,
        MachineStateIncidentCorrelation = 0.3,
        AnomalyCount = 1,
        RecommendationCount = 2,
        ComputedAtUtc = FixedUtcNow,
    };

    [Fact]
    public async Task Handle_AllSectionsSucceed_ComposesUnifiedReportFromAllThreeSources()
    {
        var kpi = CreateKpiSnapshot();
        var auditEntry = new PermissionUsageLogEntry(
            TestTenantId.Value, TestUserId.Value, "Incident.Create", "CreateIncidentCommand", "Allowed", TestCorrelationId.Value, FixedUtcNow);
        var crossSiteEntry = new CrossSiteActionLogEntryDto
        {
            CrossSiteActionLogId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            AffectedSiteIds = [Guid.NewGuid()],
            ActionType = "IncidentEscalation",
            Description = "Test escalation",
            RecordedAtUtc = FixedUtcNow,
        };

        _safetyKpiProvider.GetSectionAsync(Arg.Any<GlobalSafetyKpiSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<GlobalSafetyInsightsItemDto?>.Success(kpi));
        _auditTrailProvider.GetSectionAsync(Arg.Any<TenantAuditStreamSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<TenantAuditStreamResult>.Success(new TenantAuditStreamResult(TestTenantId.Value, FixedUtcNow, [auditEntry])));
        _crossSiteActionsProvider.GetSectionAsync(Arg.Any<CrossSiteActionLogSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<CrossSiteActionLogEntryDto>>.Success([crossSiteEntry]));

        var sut = CreateSut();
        var query = new GetComplianceReportQuery(TestTenantId.Value, FixedUtcNow.AddDays(-30), FixedUtcNow);

        var result = await sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SafetyKpiSectionAvailable.Should().BeTrue();
        result.Value.SafetyKpi.Should().Be(kpi);
        result.Value.AuditTrailSectionAvailable.Should().BeTrue();
        result.Value.AuditTrail.Should().ContainSingle().Which.Should().Be(auditEntry);
        result.Value.CrossSiteActionsSectionAvailable.Should().BeTrue();
        result.Value.CrossSiteActions.Should().ContainSingle().Which.Should().Be(crossSiteEntry);
    }

    [Fact]
    public async Task Handle_OneSectionFails_DegradesOnlyThatSectionRatherThanFailingWholeReport()
    {
        _safetyKpiProvider.GetSectionAsync(Arg.Any<GlobalSafetyKpiSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<GlobalSafetyInsightsItemDto?>.Failure(
                ErrorFactory.Create("GlobalSafetyInsights.RetrievalFailed", ErrorType.Failure, "boom")));
        _auditTrailProvider.GetSectionAsync(Arg.Any<TenantAuditStreamSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<TenantAuditStreamResult>.Success(new TenantAuditStreamResult(TestTenantId.Value, FixedUtcNow, [])));
        _crossSiteActionsProvider.GetSectionAsync(Arg.Any<CrossSiteActionLogSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<CrossSiteActionLogEntryDto>>.Success([]));

        var sut = CreateSut();
        var query = new GetComplianceReportQuery(TestTenantId.Value, FixedUtcNow.AddDays(-30), FixedUtcNow);

        var result = await sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue("a single section's failure must degrade only that section, not the whole report");
        result.Value!.SafetyKpiSectionAvailable.Should().BeFalse();
        result.Value.SafetyKpi.Should().BeNull();
        result.Value.AuditTrailSectionAvailable.Should().BeTrue();
        result.Value.CrossSiteActionsSectionAvailable.Should().BeTrue();
    }
}
