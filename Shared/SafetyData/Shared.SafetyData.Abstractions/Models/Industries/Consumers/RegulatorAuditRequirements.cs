// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           RegulatorAuditRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Regulator -> Audit requirement mapping.
//                  Introduces no new audit category type -- reuses the
//                  existing AuditChecklistItem record and
//                  AuditChecklistItemCatalog.ForRegulator exactly as defined
//                  (see AuditChecklistRegistry.cs), which are already a
//                  total, 1:1 mapping from every Regulator value to its
//                  regulatory-compliance checklist item. This file exists to
//                  give the Regulatory Subsystem its own Regulator-first
//                  entry point (ForRegulator/ByRegulator), matching the shape
//                  of RegulatorSafetyModuleMapping.cs,
//                  RegulatorSopRequirements.cs, and
//                  RegulatorPermitRequirements.cs, rather than duplicating
//                  AuditChecklistItemCatalog's derivation logic. Additive-only
//                  evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Regulator -> Audit requirement mapping, computed from
/// <see cref="AuditChecklistItemCatalog.ForRegulator"/>.
/// </summary>
public static class RegulatorAuditRequirements
{
    /// <summary>The single canonical <see cref="AuditChecklistItem"/> required by the given regulator.</summary>
    public static IReadOnlyList<AuditChecklistItem> ForRegulator(Regulator regulator) =>
        new[] { AuditChecklistItemCatalog.ForRegulator(regulator) };

    /// <summary>Audit requirements for every <see cref="Regulator"/> value, in enum declaration order.</summary>
    public static IReadOnlyDictionary<Regulator, IReadOnlyList<AuditChecklistItem>> ByRegulator { get; } =
        Enum.GetValues<Regulator>().ToDictionary(regulator => regulator, ForRegulator);

    /// <summary>
    /// Every distinct audit requirement across all of the given industry's regulators, in
    /// <see cref="IndustryDefinition.Regulators"/> order.
    /// </summary>
    public static IReadOnlyList<AuditChecklistItem> For(IndustryDefinition industry) =>
        industry.Regulators.SelectMany(ForRegulator).Distinct().ToArray();
}
