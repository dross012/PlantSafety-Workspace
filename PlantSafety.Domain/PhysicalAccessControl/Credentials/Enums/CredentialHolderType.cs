// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialHolderType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of the person a Credential is issued to.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums;

/// <summary>Classification of the person a <see cref="Credential"/> is issued to.</summary>
public enum CredentialHolderType
{
    /// <summary>Direct employee, tracked in EmployeeSafety.</summary>
    Employee = 0,

    /// <summary>Contractor, tracked in ContractorSafety.</summary>
    Contractor = 1,

    /// <summary>Visitor, tracked in ContractorVisitor.</summary>
    Visitor = 2,
}
