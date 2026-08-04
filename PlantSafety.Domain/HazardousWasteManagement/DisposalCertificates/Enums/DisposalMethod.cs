// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalMethod.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Method by which hazardous waste was ultimately disposed of / treated.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;

/// <summary>Method by which hazardous waste was ultimately disposed of / treated.</summary>
public enum DisposalMethod
{
    /// <summary>Destroyed via incineration.</summary>
    Incineration = 0,

    /// <summary>Disposed to a permitted hazardous-waste landfill.</summary>
    Landfill = 1,

    /// <summary>Recycled/reclaimed.</summary>
    Recycling = 2,

    /// <summary>Chemically or physically treated prior to disposal.</summary>
    Treatment = 3,
}
