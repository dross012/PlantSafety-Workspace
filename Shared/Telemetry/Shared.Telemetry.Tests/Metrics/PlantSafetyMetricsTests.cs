// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           PlantSafetyMetricsTests.cs
//  Created:        2026-08-09
// ============================================================================
using System.Diagnostics.Metrics;

using AwesomeAssertions;

using Shared.Telemetry.Abstractions.Metrics;

using Xunit;

namespace Shared.Telemetry.Tests.Metrics;

public sealed class PlantSafetyMetricsTests
{
    [Fact]
    public void RequestsHandled_RecordedMeasurement_IsObservableViaMeterListener()
    {
        var observed = new List<(long Value, string? Module)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == PlantSafetyMetrics.MeterName && instrument.Name == "plantsafety.requests_handled")
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            string? module = null;
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == "module")
                {
                    module = tag.Value?.ToString();
                }
            }

            observed.Add((measurement, module));
        });
        listener.Start();

        PlantSafetyMetrics.RequestsHandled.Add(1, new KeyValuePair<string, object?>("module", "facilities"));

        observed.Should().ContainSingle(m => m.Value == 1 && m.Module == "facilities");
    }

    [Fact]
    public void ErrorsHandled_And_CircuitBreakerTrips_AreDistinctInstrumentsOnTheSharedMeter()
    {
        PlantSafetyMetrics.ErrorsHandled.Should().NotBeSameAs(PlantSafetyMetrics.CircuitBreakerTrips);
        PlantSafetyMetrics.ErrorsHandled.Meter.Name.Should().Be(PlantSafetyMetrics.MeterName);
        PlantSafetyMetrics.CircuitBreakerTrips.Meter.Name.Should().Be(PlantSafetyMetrics.MeterName);
    }
}
