// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyTelemetryNormalizer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Real, working IGlobalSafetyTelemetryNormalizer implementation. Composes the
//                  platform's REAL per-site telemetry sources: AccidentManagement.Events.
//                  AccidentCreatedEvent, SafetyEvents.IncidentManagement.Events.
//                  IncidentManagementCreatedEvent (the canonical incident aggregate),
//                  Observability.Telemetry.ValueObjects.MachineStateSnapshot,
//                  EmployeeSafety.EnvironmentalHealthMonitoring.Events.
//                  EnvironmentalMonitoringMeasurementRecordedEvent (worker exposure), and
//                  Operational.ProductionTracking.Events.ProductionRunCompletedEvent (hours-worked
//                  proxy). No fabricated telemetry fields -- every SafetyTelemetryEvent field is
//                  either copied verbatim from the real source event or a documented, deliberate
//                  proxy (e.g. HoursWorkedContribution is caller-supplied because ProductionRun does
//                  not itself track headcount/hours -- see Normalize(ProductionRunCompletedEvent...)
//                  remarks). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Events;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.Observability.Telemetry.Enums;
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

namespace PlantSafety.Domain.GlobalSafetyInsights.Services;

/// <summary>Real, working normalizer composing every real per-site telemetry source into the canonical shape.</summary>
public sealed class GlobalSafetyTelemetryNormalizer : IGlobalSafetyTelemetryNormalizer, IDomainService
{
    /// <summary>Source-module discriminator for records normalized from AccidentManagement.</summary>
    public const string SourceModuleAccidentManagement = "AccidentManagement";

    /// <summary>Source-module discriminator for records normalized from SafetyEvents.IncidentManagement.</summary>
    public const string SourceModuleIncidentManagement = "SafetyEvents.IncidentManagement";

    /// <summary>Source-module discriminator for records normalized from Observability.Telemetry.</summary>
    public const string SourceModuleObservabilityTelemetry = "Observability.Telemetry";

    /// <summary>Source-module discriminator for records normalized from EmployeeSafety.EnvironmentalHealthMonitoring.</summary>
    public const string SourceModuleEnvironmentalHealthMonitoring = "EmployeeSafety.EnvironmentalHealthMonitoring";

    /// <summary>Source-module discriminator for records normalized from Operational.ProductionTracking.</summary>
    public const string SourceModuleProductionTracking = "Operational.ProductionTracking";

    /// <inheritdoc/>
    public SafetyTelemetryEvent Normalize(AccidentCreatedEvent source, Guid tenantId, Guid siteId, bool isLostTime)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new SafetyTelemetryEvent
        {
            TenantId = tenantId,
            SiteId = siteId,
            SourceModule = SourceModuleAccidentManagement,
            OccurredAtUtc = source.CreatedAtUtc,
            RecordableIncidentCount = 1,
            LostTimeIncidentCount = isLostTime ? 1 : 0,
        };
    }

    /// <inheritdoc/>
    public SafetyTelemetryEvent Normalize(IncidentManagementCreatedEvent source, Guid tenantId, Guid siteId, bool isLostTime)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new SafetyTelemetryEvent
        {
            TenantId = tenantId,
            SiteId = siteId,
            SourceModule = SourceModuleIncidentManagement,
            OccurredAtUtc = source.CreatedAtUtc,
            RecordableIncidentCount = 1,
            LostTimeIncidentCount = isLostTime ? 1 : 0,
        };
    }

    /// <inheritdoc/>
    public SafetyTelemetryEvent Normalize(MachineStateSnapshot source, Guid tenantId, Guid siteId, string machineFamily)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(machineFamily))
            throw new ArgumentException("MachineFamily must not be empty.", nameof(machineFamily));

        var degraded = source.State != MachineOperatingState.Running
                       && source.State != MachineOperatingState.Idle;

        return new SafetyTelemetryEvent
        {
            TenantId = tenantId,
            SiteId = siteId,
            SourceModule = SourceModuleObservabilityTelemetry,
            OccurredAtUtc = source.ObservedAtUtc,
            MachineFamily = machineFamily.Trim(),
            MachineStateDegraded = degraded,
        };
    }

    /// <inheritdoc/>
    public SafetyTelemetryEvent Normalize(EnvironmentalMonitoringMeasurementRecordedEvent source, Guid tenantId, Guid siteId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new SafetyTelemetryEvent
        {
            TenantId = tenantId,
            SiteId = siteId,
            SourceModule = SourceModuleEnvironmentalHealthMonitoring,
            OccurredAtUtc = source.OccurredAtUtc,
            ExposureLevelScore = source.ExposureLevelScore,
        };
    }

    /// <inheritdoc/>
    public SafetyTelemetryEvent Normalize(ProductionRunCompletedEvent source, Guid tenantId, Guid siteId, decimal hoursWorked)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (hoursWorked < 0)
            throw new ArgumentOutOfRangeException(nameof(hoursWorked), hoursWorked, "HoursWorked must not be negative.");

        // ProductionRun tracks unit counts/status, not headcount or hours directly -- the caller
        // (Infrastructure consumer) supplies the real hours-worked figure it independently resolves
        // (e.g. from the production line's shift roster), so this normalizer never fabricates a
        // headcount*duration estimate with no backing data.
        return new SafetyTelemetryEvent
        {
            TenantId = tenantId,
            SiteId = siteId,
            SourceModule = SourceModuleProductionTracking,
            OccurredAtUtc = source.EndedAtUtc,
            HoursWorkedContribution = hoursWorked,
        };
    }
}
