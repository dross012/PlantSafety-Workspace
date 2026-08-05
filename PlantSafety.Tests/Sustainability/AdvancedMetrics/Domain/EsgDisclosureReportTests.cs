// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Domain
//  Layer:          Tests (Unit)
//  File:           EsgDisclosureReportTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the EsgDisclosureReport aggregate root: Generate
//                  guards, Publish (immutability boundary), and CreateSupersedingVersion (the only
//                  correction path for a published report). Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Exceptions;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Domain;

public sealed class EsgDisclosureReportTests : UnitTestBase
{
    private static readonly UtcTimestamp PeriodStart = UtcTimestamp.From(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly UtcTimestamp PeriodEnd = UtcTimestamp.From(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc));

    private EsgDisclosureReport DraftReport(EsgDisclosureFramework frameworks = EsgDisclosureFramework.GhgProtocol) =>
        EsgDisclosureReport.Generate(
            generatedBy: TestUserId, generatedAtUtc: FixedTimestamp, correlationId: TestCorrelationId, causationId: TestCausationId,
            actor: TestActor, tenantId: TestTenantId.Value, siteId: null, sourceLedgerId: AdvancedMetricsLedgerId.New(),
            frameworks: frameworks, reportingPeriodStartUtc: PeriodStart, reportingPeriodEndUtc: PeriodEnd,
            methodologyVersion: "GHG-Protocol-2026.1");

    // ---- Generate -------------------------------------------------------------

    [Fact]
    public void Generate_WithValidArguments_CreatesVersion1DraftAndRaisesEvent()
    {
        var report = DraftReport();

        report.Version.Should().Be(1);
        report.SupersedesReportId.Should().BeNull();
        report.Status.Should().Be(EsgDisclosureReportStatus.Draft);
        report.PublishedAtUtc.Should().BeNull();
        report.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<EsgDisclosureReportGeneratedEvent>();
    }

    [Fact]
    public void Generate_WithEmptyTenantId_Throws()
    {
        var act = () => EsgDisclosureReport.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: null, sourceLedgerId: AdvancedMetricsLedgerId.New(),
            frameworks: EsgDisclosureFramework.GhgProtocol, reportingPeriodStartUtc: PeriodStart, reportingPeriodEndUtc: PeriodEnd,
            methodologyVersion: "v1");

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Fact]
    public void Generate_WithNoFrameworkSelected_Throws()
    {
        var act = () => EsgDisclosureReport.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, sourceLedgerId: AdvancedMetricsLedgerId.New(),
            frameworks: EsgDisclosureFramework.None, reportingPeriodStartUtc: PeriodStart, reportingPeriodEndUtc: PeriodEnd,
            methodologyVersion: "v1");

        act.Should().Throw<ArgumentException>().WithParameterName("frameworks");
    }

    [Fact]
    public void Generate_WithBlankMethodologyVersion_Throws()
    {
        var act = () => EsgDisclosureReport.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, sourceLedgerId: AdvancedMetricsLedgerId.New(),
            frameworks: EsgDisclosureFramework.GhgProtocol, reportingPeriodStartUtc: PeriodStart, reportingPeriodEndUtc: PeriodEnd,
            methodologyVersion: "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("methodologyVersion");
    }

    [Fact]
    public void Generate_WithMultipleFrameworkFlags_PreservesCombinedFlags()
    {
        var report = DraftReport(EsgDisclosureFramework.GhgProtocol | EsgDisclosureFramework.Csrd);

        report.Frameworks.Should().HaveFlag(EsgDisclosureFramework.GhgProtocol);
        report.Frameworks.Should().HaveFlag(EsgDisclosureFramework.Csrd);
        report.Frameworks.Should().NotHaveFlag(EsgDisclosureFramework.SecClimateDisclosure);
    }

    // ---- Publish -------------------------------------------------------------

    [Fact]
    public void Publish_WhenDraft_TransitionsToPublishedAndStampsTimestamp()
    {
        var report = DraftReport();
        var publishedAt = UtcTimestamp.From(FixedUtcNow.AddDays(1));

        report.Publish(publishedAt, TestActor, TestCorrelationId, TestCausationId);

        report.Status.Should().Be(EsgDisclosureReportStatus.Published);
        report.PublishedAtUtc.Should().Be(publishedAt);
        report.DomainEvents.OfType<EsgDisclosureReportPublishedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ThrowsInvalidTransition()
    {
        var report = DraftReport();
        report.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => report.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<EsgDisclosureReportInvalidStatusTransitionException>();
    }

    // ---- CreateSupersedingVersion -------------------------------------------------

    [Fact]
    public void CreateSupersedingVersion_OnDraftReport_ThrowsInvalidTransition()
    {
        var report = DraftReport();

        var act = () => report.CreateSupersedingVersion(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            AdvancedMetricsLedgerId.New(), "GHG-Protocol-2026.2");

        act.Should().Throw<EsgDisclosureReportInvalidStatusTransitionException>(
            "only a Published report may be superseded -- correcting a Draft is done in place");
    }

    [Fact]
    public void CreateSupersedingVersion_OnPublishedReport_ProducesLinkedNextVersionAndMarksOriginalSuperseded()
    {
        var report = DraftReport();
        report.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        report.ClearDomainEvents();

        var newLedgerId = AdvancedMetricsLedgerId.New();
        var next = report.CreateSupersedingVersion(
            TestUserId, UtcTimestamp.From(FixedUtcNow.AddDays(2)), TestCorrelationId, TestCausationId, TestActor,
            newLedgerId, "GHG-Protocol-2026.2");

        report.Status.Should().Be(EsgDisclosureReportStatus.Superseded);
        report.DomainEvents.OfType<EsgDisclosureReportSupersededEvent>().Should().ContainSingle();

        next.Version.Should().Be(2);
        next.SupersedesReportId.Should().Be(report.EsgDisclosureReportId);
        next.Status.Should().Be(EsgDisclosureReportStatus.Draft);
        next.SourceLedgerId.Should().Be(newLedgerId);
        next.MethodologyVersion.Should().Be("GHG-Protocol-2026.2");
        next.TenantId.Should().Be(report.TenantId);
        next.DomainEvents.OfType<EsgDisclosureReportGeneratedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CreateSupersedingVersion_WithNullMethodologyVersion_InheritsFromOriginal()
    {
        var report = DraftReport();
        report.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var next = report.CreateSupersedingVersion(
            TestUserId, UtcTimestamp.From(FixedUtcNow.AddDays(2)), TestCorrelationId, TestCausationId, TestActor,
            AdvancedMetricsLedgerId.New(), methodologyVersion: null!);

        next.MethodologyVersion.Should().Be(report.MethodologyVersion);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var report = DraftReport();

        report.ClearDomainEvents();

        report.DomainEvents.Should().BeEmpty();
    }
}
