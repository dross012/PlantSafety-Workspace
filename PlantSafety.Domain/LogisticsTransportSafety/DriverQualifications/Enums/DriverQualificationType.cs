// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of driver qualification/credential types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Enums;

/// <summary>Classification of driver qualification/credential types.</summary>
public enum DriverQualificationType
{
    /// <summary>Commercial Driver's License.</summary>
    CommercialDriverLicense = 0,

    /// <summary>Hazardous materials transport endorsement.</summary>
    HazmatEndorsement = 1,

    /// <summary>Powered industrial truck (forklift) certification.</summary>
    ForkliftCertified = 2,

    /// <summary>Defensive driving certification.</summary>
    DefensiveDrivingCertified = 3,
}
