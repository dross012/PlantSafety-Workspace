// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           PackagingStatusResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the PackagingSpecification lifecycle-transition
//                  commands (Supersede, Retire). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Result payload shared by PackagingSpecification lifecycle-transition commands.</summary>
/// <param name="PackagingSpecificationId">Identity of the affected PackagingSpecification aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record PackagingStatusResult(Guid PackagingSpecificationId, string Status);
