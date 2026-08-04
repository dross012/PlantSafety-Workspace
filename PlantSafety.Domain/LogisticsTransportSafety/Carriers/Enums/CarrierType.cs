// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of fleet and carrier relationship types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Enums;

/// <summary>Classification of carrier relationship types.</summary>
public enum CarrierType
{
    /// <summary>Vehicles and drivers owned/operated directly by the plant.</summary>
    InHouseFleet = 0,

    /// <summary>Third-party contract carrier under a service agreement.</summary>
    ThirdPartyContractCarrier = 1,

    /// <summary>Independent owner-operator engaged on a per-trip basis.</summary>
    OwnerOperator = 2,
}
