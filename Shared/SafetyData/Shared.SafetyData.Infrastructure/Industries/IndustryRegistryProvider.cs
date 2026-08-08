// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           IndustryRegistryProvider.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-resolvable IIndustryRegistry implementation that delegates
//                  every member to the static IndustryRegistry (Abstractions.
//                  Models.Industries). Holds no state of its own -- the taxonomy
//                  data lives exclusively in IndustryRegistry, which this class
//                  does not modify. Stateless, so a single instance is safe to
//                  share as a singleton.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>
/// Stateless <see cref="IIndustryRegistry"/> implementation backed by the
/// static <see cref="IndustryRegistry"/>.
/// </summary>
public sealed class IndustryRegistryProvider : IIndustryRegistry
{
    /// <inheritdoc />
    public IReadOnlyList<IndustryDefinition> All => IndustryRegistry.All;

    /// <inheritdoc />
    public IndustryDefinition? FindByName(string name) => IndustryRegistry.FindByName(name);

    /// <inheritdoc />
    public IReadOnlyList<IndustryDefinition> ByTopLevelCategory(string topLevelCategory) =>
        IndustryRegistry.ByTopLevelCategory(topLevelCategory);
}
