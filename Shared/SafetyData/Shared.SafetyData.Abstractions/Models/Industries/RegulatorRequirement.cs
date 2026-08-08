// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           RegulatorRequirement.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable schema for a single named regulatory requirement
//                  within the Regulatory Subsystem. Requirement instances
//                  themselves are enumerated in RegulatorRequirementCatalog.cs
//                  (each tied to an existing Regulator value, and to the
//                  RegulatorCategory that Regulator maps onto via
//                  RegulatorCategoryCatalog.ForRegulator). Mirrors
//                  IndustryDefinition.cs's shape/placement. Additive-only
//                  evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// A single named regulatory requirement drawn from a specific <see cref="Regulator"/>'s
/// real regulatory scope.
/// </summary>
/// <param name="Name">The requirement's canonical, short name (e.g. "Lockout/Tagout (LOTO)").</param>
/// <param name="Description">Human-readable description of what the requirement obligates.</param>
/// <param name="Regulator">The specific regulator this requirement is drawn from.</param>
/// <param name="Category">The <see cref="RegulatorCategory"/> <paramref name="Regulator"/> maps onto (<see cref="RegulatorCategoryCatalog.ForRegulator"/>).</param>
public sealed record RegulatorRequirement(
    string Name,
    string Description,
    Regulator Regulator,
    RegulatorCategory Category);
