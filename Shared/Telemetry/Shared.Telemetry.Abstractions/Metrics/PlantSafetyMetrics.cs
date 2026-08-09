// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Metrics
//  File:           PlantSafetyMetrics.cs
//  Created:        2026-08-09
//  Description:    Phase 3.3 deliverable: the one Meter every host in this
//                  repo publishes counters through. Built entirely on
//                  System.Diagnostics.Metrics (BCL, no NuGet package, no
//                  OTLP endpoint or Prometheus collector required) -- this
//                  sandbox has neither, so counters are observable today via
//                  `dotnet-counters monitor --process-id <pid> --counters
//                  PlantSafety` without standing up any collector, and the
//                  same Meter can be exported to a real collector later via
//                  OpenTelemetry.Extensions.Hosting's AddMeter("PlantSafety")
//                  without touching call sites.
// ============================================================================
using System.Diagnostics.Metrics;

namespace Shared.Telemetry.Abstractions.Metrics;

/// <summary>The shared Meter and counters every host publishes request/error/circuit-breaker metrics through.</summary>
public static class PlantSafetyMetrics
{
    /// <summary>The Meter name hosts register with an exporter (e.g. AddMeter("PlantSafety")).</summary>
    public const string MeterName = "PlantSafety";

    private static readonly Meter Meter = new(MeterName, "1.0");

    /// <summary>Total requests handled, tagged by module.</summary>
    public static readonly Counter<long> RequestsHandled = Meter.CreateCounter<long>(
        "plantsafety.requests_handled", unit: "{request}", description: "Total requests handled, tagged by module.");

    /// <summary>Total unhandled exceptions converted to ErrorEnvelope responses, tagged by module and error category.</summary>
    public static readonly Counter<long> ErrorsHandled = Meter.CreateCounter<long>(
        "plantsafety.errors_handled", unit: "{error}", description: "Total unhandled exceptions handled, tagged by module and error category.");

    /// <summary>Total times a circuit breaker recorded a failure that tripped it open.</summary>
    public static readonly Counter<long> CircuitBreakerTrips = Meter.CreateCounter<long>(
        "plantsafety.circuit_breaker_trips", unit: "{trip}", description: "Total circuit breaker trips, tagged by module.");
}
