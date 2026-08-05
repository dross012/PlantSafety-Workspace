// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Domain
//  Layer:          Tests (Unit)
//  File:           LifecycleRulesTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Exhaustive truth-table tests for AdvancedMetricsLedgerLifecycleRules and
//                  EsgDisclosureReportLifecycleRules -- the pure policy classes the aggregates
//                  delegate transition-permission checks to. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Rules;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Domain;

public sealed class LifecycleRulesTests
{
    [Theory]
    [InlineData(AdvancedMetricsLedgerStatus.Open, true)]
    [InlineData(AdvancedMetricsLedgerStatus.Closed, false)]
    [InlineData(AdvancedMetricsLedgerStatus.Archived, false)]
    public void AdvancedMetricsLedgerLifecycleRules_CanRecordMetrics_OnlyWhenOpen(AdvancedMetricsLedgerStatus status, bool expected)
        => AdvancedMetricsLedgerLifecycleRules.CanRecordMetrics(status).Should().Be(expected);

    [Theory]
    [InlineData(AdvancedMetricsLedgerStatus.Open, true)]
    [InlineData(AdvancedMetricsLedgerStatus.Closed, false)]
    [InlineData(AdvancedMetricsLedgerStatus.Archived, false)]
    public void AdvancedMetricsLedgerLifecycleRules_CanClose_OnlyFromOpen(AdvancedMetricsLedgerStatus status, bool expected)
        => AdvancedMetricsLedgerLifecycleRules.CanClose(status).Should().Be(expected);

    [Theory]
    [InlineData(AdvancedMetricsLedgerStatus.Open, false)]
    [InlineData(AdvancedMetricsLedgerStatus.Closed, true)]
    [InlineData(AdvancedMetricsLedgerStatus.Archived, false)]
    public void AdvancedMetricsLedgerLifecycleRules_CanArchive_OnlyFromClosed(AdvancedMetricsLedgerStatus status, bool expected)
        => AdvancedMetricsLedgerLifecycleRules.CanArchive(status).Should().Be(expected);

    [Theory]
    [InlineData(EsgDisclosureReportStatus.Draft, true)]
    [InlineData(EsgDisclosureReportStatus.Published, false)]
    [InlineData(EsgDisclosureReportStatus.Superseded, false)]
    public void EsgDisclosureReportLifecycleRules_CanPublish_OnlyFromDraft(EsgDisclosureReportStatus status, bool expected)
        => EsgDisclosureReportLifecycleRules.CanPublish(status).Should().Be(expected);

    [Theory]
    [InlineData(EsgDisclosureReportStatus.Draft, false)]
    [InlineData(EsgDisclosureReportStatus.Published, true)]
    [InlineData(EsgDisclosureReportStatus.Superseded, false)]
    public void EsgDisclosureReportLifecycleRules_CanSupersede_OnlyFromPublished(EsgDisclosureReportStatus status, bool expected)
        => EsgDisclosureReportLifecycleRules.CanSupersede(status).Should().Be(expected);
}
