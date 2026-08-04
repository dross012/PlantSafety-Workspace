// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of on-site material/personnel transport vehicle types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Enums;

/// <summary>Classification of transport vehicle types tracked by LogisticsTransportSafety.</summary>
public enum TransportVehicleType
{
    /// <summary>On-site or over-the-road truck.</summary>
    Truck = 0,

    /// <summary>Powered industrial forklift.</summary>
    Forklift = 1,

    /// <summary>Towed trailer.</summary>
    Trailer = 2,

    /// <summary>Personnel transport van.</summary>
    Van = 3,

    /// <summary>Heavy equipment carrier/lowboy.</summary>
    HeavyEquipmentCarrier = 4,
}
