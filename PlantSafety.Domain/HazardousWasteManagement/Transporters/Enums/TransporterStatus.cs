// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the Transporter aggregate root (EPA-registered
//                  hazardous-waste transporter).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Enums;

/// <summary>Lifecycle states for the Transporter aggregate root.</summary>
public enum TransporterStatus
{
    /// <summary>Transporter active and eligible for pickups.</summary>
    Active = 0,

    /// <summary>Transporter suspended (e.g. expired EPA license).</summary>
    Suspended = 1,
}
