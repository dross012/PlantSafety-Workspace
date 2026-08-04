// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the TransportVehicle aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="TransportVehicle"/> aggregate.</summary>
public static class TransportVehicleLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an inspection may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleInspection(TransportVehicleStatus current) =>
        current is TransportVehicleStatus.Registered or TransportVehicleStatus.InspectionPassed or TransportVehicleStatus.InspectionFailed;

    /// <summary>Returns <see langword="true"/> when an inspection result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordInspectionResult(TransportVehicleStatus current) => current == TransportVehicleStatus.InspectionScheduled;

    /// <summary>Returns <see langword="true"/> when the vehicle may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(TransportVehicleStatus current) => current != TransportVehicleStatus.Retired;
}
