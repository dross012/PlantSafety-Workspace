// ============================================================================
//  Test IDs:  PLNT-RESL-SENSOR-001..003
//  Manual:    tests_rules_design_manual.md Ch.9 ("Sensor gap detection: >10s
//             gap -> alert within 15s; fail-safe default states mandatory").
//  Purpose:   Pins the sensor-gap hard numbers with an executable reference
//             monitor. NOTE: production (PlantSafety.Domain/Infrastructure)
//             does not yet contain a literal "sensor heartbeat" component --
//             this is a documented gap. SensorGapMonitor below is a minimal,
//             deterministic reference implementation (synthetic timestamps,
//             no real clock/sleep) that encodes the required behavior so the
//             threshold numbers are captured by an executable spec pending
//             the production implementation.
// ============================================================================
namespace PlantSafety.ResilienceTests.Sensors;

/// <summary>Deterministic reference monitor for the sensor-gap hard numbers (Ch.9).</summary>
internal sealed class SensorGapMonitor(TimeSpan gapThreshold, TimeSpan alertWithin)
{
    /// <summary>
    /// Returns true if, given the last-seen sensor timestamp and the current time, a gap
    /// beyond <paramref name="gapThreshold"/> exists AND detection would occur within
    /// <paramref name="alertWithin"/> of the gap threshold being crossed.
    /// </summary>
    public (bool GapDetected, bool WithinAlertWindow) Evaluate(DateTime lastSeenUtc, DateTime nowUtc, DateTime detectedAtUtc)
    {
        var gap = nowUtc - lastSeenUtc;
        var gapDetected = gap > gapThreshold;

        if (!gapDetected)
            return (false, true);

        var gapCrossedAtUtc = lastSeenUtc + gapThreshold;
        var detectionLatency = detectedAtUtc - gapCrossedAtUtc;
        return (true, detectionLatency <= alertWithin);
    }

    /// <summary>Fail-safe default state used when no valid sensor reading exists at all.</summary>
    public static SensorState FailSafeDefault => SensorState.Isolated;
}

internal enum SensorState { Unknown, Nominal, Isolated }

public sealed class SensorGapDetectionResilienceTests
{
    private static readonly SensorGapMonitor Monitor = new(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15));

    [Fact(DisplayName = "PLNT-RESL-SENSOR-001: A gap of exactly 10s is NOT yet a violation (threshold is exclusive)")]
    public void TenSecondGap_ShouldNotYetBeAViolation()
    {
        var lastSeen = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc);
        var now = lastSeen.AddSeconds(10);

        var (gapDetected, _) = Monitor.Evaluate(lastSeen, now, now);

        gapDetected.Should().BeFalse("tests_rules_design_manual.md specifies '>10s', so exactly 10s must not trigger");
    }

    [Fact(DisplayName = "PLNT-RESL-SENSOR-002: A gap of 10.1s IS a violation and is detected within the 15s alert window")]
    public void JustOverTenSecondGap_ShouldBeDetected_WithinFifteenSeconds()
    {
        var lastSeen = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc);
        var now = lastSeen.AddSeconds(10.1);
        var detectedAt = lastSeen.AddSeconds(10.1 + 5); // detection 5s after crossing threshold

        var (gapDetected, withinWindow) = Monitor.Evaluate(lastSeen, now, detectedAt);

        gapDetected.Should().BeTrue();
        withinWindow.Should().BeTrue("detection occurred well within the mandated 15s alert window");
    }

    [Fact(DisplayName = "PLNT-RESL-SENSOR-003: Detection later than 15s after the gap crossed the threshold is a Critical defect (out of SLA)")]
    public void GapDetectedAfterFifteenSeconds_ShouldBeOutOfSla()
    {
        var lastSeen = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc);
        var now = lastSeen.AddSeconds(11);
        var detectedAt = lastSeen.AddSeconds(11 + 16); // 16s after crossing -> exceeds 15s SLA

        var (gapDetected, withinWindow) = Monitor.Evaluate(lastSeen, now, detectedAt);

        gapDetected.Should().BeTrue();
        withinWindow.Should().BeFalse("detection more than 15s after the gap crossed the threshold violates the hard SLA number");
    }

    [Fact(DisplayName = "PLNT-RESL-SENSOR-004: With no valid reading at all, the fail-safe default state is Isolated (never assumed-nominal)")]
    public void NoReadingAtAll_ShouldFailSafeToIsolated()
    {
        SensorGapMonitor.FailSafeDefault.Should().Be(SensorState.Isolated,
            "tests_rules_design_manual.md Ch.9 mandates fail-safe default states -- a missing sensor must never be assumed safe/nominal");
    }
}
