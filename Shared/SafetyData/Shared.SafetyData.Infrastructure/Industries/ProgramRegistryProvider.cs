// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           ProgramRegistryProvider.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-resolvable IProgramRegistry implementation that
//                  delegates every member to the static ProgramCatalog and
//                  ProgramRequirementMapping (Abstractions.Models.Industries).
//                  Holds no state of its own -- mirrors
//                  RegulatoryRequirementRegistryProvider and
//                  IndustryRegistryProvider exactly. Stateless, so a single
//                  instance is safe to share as a singleton.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>
/// Stateless <see cref="IProgramRegistry"/> implementation backed by the static
/// <see cref="ProgramCatalog"/> and <see cref="ProgramRequirementMapping"/>.
/// </summary>
public sealed class ProgramRegistryProvider : IProgramRegistry
{
    /// <inheritdoc />
    public IReadOnlyList<ProgramDefinition> All => ProgramCatalog.All;

    /// <inheritdoc />
    public IReadOnlyList<ProgramDefinition> ForRegulator(Regulator regulator) =>
        ProgramCatalog.ForRegulator(regulator);

    /// <inheritdoc />
    public IReadOnlyList<ProgramDefinition> ForFlag(SafetyFlag flag) =>
        ProgramCatalog.ForFlag(flag);

    /// <inheritdoc />
    public ProgramDefinition? FindByCode(string code) =>
        ProgramCatalog.FindByCode(code);

    /// <inheritdoc />
    public IReadOnlyList<ProgramDefinition> ForIndustry(IndustryDefinition industry) =>
        ProgramRequirementMapping.For(industry);

    /// <inheritdoc />
    public IReadOnlyList<ProgramDefinition> ForIndustryName(string name) =>
        ProgramRequirementMapping.ByIndustryName(name);
}
