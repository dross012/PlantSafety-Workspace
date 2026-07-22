// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalMonitoringCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of environmental/health monitoring categories per the PlantSafety
//                  Program Structure Environmental & Health Monitoring component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Enums;

/// <summary>Classification of environmental/health monitoring categories.</summary>
public enum EnvironmentalMonitoringCategory
{
    /// <summary>Ventilation / air quality monitoring.</summary>
    AirQuality = 0,

    /// <summary>Noise level assessment / hearing conservation monitoring.</summary>
    Noise = 1,

    /// <summary>Vibration exposure monitoring for tools and equipment.</summary>
    Vibration = 2,

    /// <summary>Respiratory protection / airborne contaminant exposure monitoring.</summary>
    RespiratoryExposure = 3,
}
