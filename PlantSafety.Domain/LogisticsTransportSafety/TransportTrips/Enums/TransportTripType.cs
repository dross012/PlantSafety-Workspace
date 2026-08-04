// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of transport trip purposes.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Enums;

/// <summary>Classification of transport trip purposes.</summary>
public enum TransportTripType
{
    /// <summary>On-site movement of material between plant locations.</summary>
    OnSiteMaterialMove = 0,

    /// <summary>Off-site delivery to/from an external location.</summary>
    OffSiteDelivery = 1,

    /// <summary>Transport of personnel rather than material.</summary>
    PersonnelTransport = 2,

    /// <summary>Transport of hazardous materials, subject to hazmat-transport rules.</summary>
    HazmatTransport = 3,
}
