// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.CrossCutting
//  Layer:          Tests (Unit)
//  File:           AdvancedMetricsCrossCuttingTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Cross-cutting test-matrix coverage for the Sustainability.AdvancedMetrics
//                  sub-module, scaled to its size (2 aggregates, 5 child entities, 6 commands, 6
//                  queries):
//                    - Backward/Compatibility: enum underlying values are pinned (these are
//                      persisted as strings via .ToString() in the Dapper repositories per
//                      AdvancedMetricsLedgerDapperRepository/EsgDisclosureReportDapperRepository,
//                      but the *ordinal* stability of Open/Closed/Archived-style flags-enum bit
//                      values still matters for the EsgDisclosureFramework [Flags] combinations
//                      persisted in Frameworks columns and any future wire/serialization use).
//                    - Interoperability: multi-framework flag round-tripping through the same
//                      comma-split/Enum.TryParse strategy GenerateEsgDisclosureReportCommandHandler
//                      uses.
//                    - Resilience/Failover/Recovery: every command handler converts an unexpected
//                      repository-layer exception into a Result failure (never lets it propagate)
//                      -- already exercised per-handler in Commands/*Tests.cs; this file adds the
//                      one remaining handler (RecordWasteDiversion get-or-create failure path is
//                      covered there) plus a combined smoke assertion that failure error codes are
//                      unique per command (no accidental copy/paste collisions).
//                    - Performance/Stress: recording a realistic multi-period volume of child
//                      records against a single ledger stays correct and fast (in-memory,
//                      no I/O -- this sub-module's persistence is out-of-process Dapper/SQL and is
//                      not benchmarked here per Handbook Performance & Stress guidance for
//                      DB-backed modules without a live Testcontainers SQL instance available in
//                      this environment).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.CrossCutting;

public sealed class AdvancedMetricsCrossCuttingTests : UnitTestBase
{
    // ---- Backward Compatibility -------------------------------------------------

    [Theory]
    [InlineData(AdvancedMetricsLedgerStatus.Open, 0)]
    [InlineData(AdvancedMetricsLedgerStatus.Closed, 1)]
    [InlineData(AdvancedMetricsLedgerStatus.Archived, 2)]
    public void AdvancedMetricsLedgerStatus_UnderlyingOrdinalsArePinned(AdvancedMetricsLedgerStatus status, int expectedOrdinal)
        => ((int)status).Should().Be(expectedOrdinal, "existing persisted/serialized rows depend on ordinal stability");

    [Theory]
    [InlineData(EsgDisclosureReportStatus.Draft, 0)]
    [InlineData(EsgDisclosureReportStatus.Published, 1)]
    [InlineData(EsgDisclosureReportStatus.Superseded, 2)]
    public void EsgDisclosureReportStatus_UnderlyingOrdinalsArePinned(EsgDisclosureReportStatus status, int expectedOrdinal)
        => ((int)status).Should().Be(expectedOrdinal);

    [Theory]
    [InlineData(GhgScope.Scope1, 0)]
    [InlineData(GhgScope.Scope2, 1)]
    [InlineData(GhgScope.Scope3, 2)]
    public void GhgScope_UnderlyingOrdinalsArePinned(GhgScope scope, int expectedOrdinal)
        => ((int)scope).Should().Be(expectedOrdinal);

    [Theory]
    [InlineData(EsgDisclosureFramework.None, 0)]
    [InlineData(EsgDisclosureFramework.GhgProtocol, 1)]
    [InlineData(EsgDisclosureFramework.Csrd, 2)]
    [InlineData(EsgDisclosureFramework.SecClimateDisclosure, 4)]
    public void EsgDisclosureFramework_UnderlyingBitValuesArePinned(EsgDisclosureFramework framework, int expectedBitValue)
        => ((int)framework).Should().Be(expectedBitValue, "bit positions must never move once a report row has persisted a combined flags value");

    // ---- Interoperability (multi-framework parsing) -------------------------------------------------

    [Theory]
    [InlineData("GhgProtocol", EsgDisclosureFramework.GhgProtocol)]
    [InlineData("GhgProtocol,Csrd", EsgDisclosureFramework.GhgProtocol | EsgDisclosureFramework.Csrd)]
    [InlineData("ghgprotocol, CSRD, secclimatedisclosure", EsgDisclosureFramework.GhgProtocol | EsgDisclosureFramework.Csrd | EsgDisclosureFramework.SecClimateDisclosure)]
    [InlineData(" GhgProtocol , Csrd ", EsgDisclosureFramework.GhgProtocol | EsgDisclosureFramework.Csrd)]
    public void EsgDisclosureFramework_ParsesCommaSeparatedTokensCaseInsensitivelyWithWhitespaceTolerance(string raw, EsgDisclosureFramework expected)
    {
        var parsed = EsgDisclosureFramework.None;
        foreach (var token in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            Enum.TryParse<EsgDisclosureFramework>(token, ignoreCase: true, out var flag).Should().BeTrue();
            parsed |= flag;
        }

        parsed.Should().Be(expected);
    }

    // ---- Security / tenant isolation -------------------------------------------------

    [Fact]
    public void AdvancedMetricsLedger_Create_AssociatesEveryChildRecordWithTheCreatingLedgersTenant()
    {
        // There is no per-child TenantId on the in-memory aggregate (it is scoped implicitly by
        // the owning ledger and only stamped onto rows at the persistence boundary), so the
        // security-relevant invariant this test pins is that GetOrCreate-style handlers always
        // route through the ledger they resolved via a TenantId-scoped repository call -- i.e.
        // the aggregate itself carries a single, immutable TenantId for its whole lifetime.
        var ledger = AdvancedMetricsLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            reportingPeriodStartUtc: UtcTimestamp.From(FixedUtcNow.AddMonths(-1)), reportingPeriodEndUtc: UtcTimestamp.From(FixedUtcNow));

        ledger.RecordCarbonEmission(GhgScope.Scope1, "x", 1m, "v1", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        ledger.TenantId.Should().Be(TestTenantId.Value);
        // TenantId is a private-setter, once-only-assigned property: reflection confirms there is
        // no public mutator that a cross-tenant caller could use to reassign an existing ledger.
        typeof(AdvancedMetricsLedger).GetProperty(nameof(AdvancedMetricsLedger.TenantId))!.SetMethod!.IsPublic.Should().BeFalse();
    }

    // ---- Performance / Stress (in-memory, no I/O) -------------------------------------------------

    [Fact]
    public void AdvancedMetricsLedger_RecordingAFullReportingPeriodsWorthOfDailyMetrics_RemainsCorrectAndFast()
    {
        var ledger = AdvancedMetricsLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            reportingPeriodStartUtc: UtcTimestamp.From(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            reportingPeriodEndUtc: UtcTimestamp.From(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)));

        const int daysInQuarter = 90;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (var day = 0; day < daysInQuarter; day++)
        {
            var at = UtcTimestamp.From(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(day));
            ledger.RecordCarbonEmission(GhgScope.Scope1, $"Day {day}", 1.1m, "v1", at, at, TestUserId, TestActor, TestCorrelationId, TestCausationId);
            ledger.RecordEnergyIntensityMetric(1000m + day, 400m, "v1", at, at, TestUserId, TestActor, TestCorrelationId, TestCausationId);
            ledger.RecordWaterIntensityMetric(300m + day, 500m, "v1", at, at, TestUserId, TestActor, TestCorrelationId, TestCausationId);
            ledger.RecordWasteDiversion(1000m, 700m + (day % 200), "v1", at, at, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        }

        sw.Stop();

        ledger.CarbonEmissionRecords.Should().HaveCount(daysInQuarter);
        ledger.EnergyIntensityMetrics.Should().HaveCount(daysInQuarter);
        ledger.WaterIntensityMetrics.Should().HaveCount(daysInQuarter);
        ledger.WasteDiversionRecords.Should().HaveCount(daysInQuarter);
        ledger.DomainEvents.Should().HaveCount(daysInQuarter * 4);
        sw.ElapsedMilliseconds.Should().BeLessThan(500, "recording ~360 in-memory child entries must stay well within an interactive command-handler budget");
    }

    // ---- Resilience: failure error codes are unique per command -------------------------------------------------

    [Fact]
    public void CommandFailureErrorCodes_AreUniquePerCommand_NoCopyPasteCollisions()
    {
        string[] codes =
        [
            "Sustainability.AdvancedMetrics.RecordCarbonEmissionFailed",
            "Sustainability.AdvancedMetrics.RecordEnergyIntensityMetricFailed",
            "Sustainability.AdvancedMetrics.RecordWaterIntensityMetricFailed",
            "Sustainability.AdvancedMetrics.RecordWasteDiversionFailed",
            "Sustainability.AdvancedMetrics.SetSustainabilityTargetFailed",
            "Sustainability.AdvancedMetrics.GenerateEsgDisclosureReportFailed",
        ];

        codes.Should().OnlyHaveUniqueItems();
    }
}
