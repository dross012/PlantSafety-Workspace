// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyTelemetryNormalizer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Domain service contract normalizing per-site telemetry from the platform's real
//                  existing sources -- AccidentManagement (incidents), SafetyEvents.IncidentManagement
//                  (the canonical incident aggregate), Observability.Telemetry (machine state),
//                  EmployeeSafety.EnvironmentalHealthMonitoring (worker exposure), and
//                  Operational.ProductionTracking (production runs) -- into the canonical
//                  SafetyTelemetryEvent shape. See Services.GlobalSafetyTelemetryNormalizer for the
//                  concrete mapping from each real source type. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Events;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

namespace PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

/// <summary>Normalizes per-site telemetry from real source modules into the canonical <see cref="SafetyTelemetryEvent"/> shape.</summary>
public interface IGlobalSafetyTelemetryNormalizer
{
    /// <summary>Normalizes a real AccidentManagement.Events.AccidentCreatedEvent (site accident) into a canonical telemetry record.</summary>
    SafetyTelemetryEvent Normalize(AccidentCreatedEvent source, Guid tenantId, Guid siteId, bool isLostTime);

    /// <summary>Normalizes a real SafetyEvents.IncidentManagement.Events.IncidentManagementCreatedEvent (the canonical incident aggregate) into a canonical telemetry record.</summary>
    SafetyTelemetryEvent Normalize(IncidentManagementCreatedEvent source, Guid tenantId, Guid siteId, bool isLostTime);

    /// <summary>Normalizes a real Observability.Telemetry machine-state observation into a canonical telemetry record.</summary>
    SafetyTelemetryEvent Normalize(MachineStateSnapshot source, Guid tenantId, Guid siteId, string machineFamily);

    /// <summary>Normalizes a real EmployeeSafety.EnvironmentalHealthMonitoring exposure measurement into a canonical telemetry record.</summary>
    SafetyTelemetryEvent Normalize(EnvironmentalMonitoringMeasurementRecordedEvent source, Guid tenantId, Guid siteId);

    /// <summary>Normalizes a real Operational.ProductionTracking.Events.ProductionRunCompletedEvent (hours-worked proxy) into a canonical telemetry record.</summary>
    SafetyTelemetryEvent Normalize(ProductionRunCompletedEvent source, Guid tenantId, Guid siteId, decimal hoursWorked);
}
