// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real vetting lifecycle states for the PreQualificationVetting aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Enums;

/// <summary>Lifecycle states for the <see cref="PreQualificationVetting"/> aggregate root.</summary>
public enum PreQualificationVettingStatus
{
    /// <summary>Contractor submitted for pre-qualification review.</summary>
    SubmittedForReview = 0,

    /// <summary>Vetting review in progress (safety metrics, certifications, insurance, OSHA compliance).</summary>
    UnderReview = 1,

    /// <summary>Contractor approved to work at the facility.</summary>
    Approved = 2,

    /// <summary>Contractor rejected; did not meet minimum safety/compliance standards.</summary>
    Rejected = 3,

    /// <summary>Approval has expired; annual re-vetting required.</summary>
    Expired = 4,
}
