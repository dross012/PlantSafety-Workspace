/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               OpenInvestigationSpecification.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.AIInvestigations.Aggregates;

namespace Intelligence.Domain.AIInvestigations.Specifications;

/// <summary>Satisfied when an investigation is in a non-terminal status.</summary>
public sealed class OpenInvestigationSpecification : ISpecification<Investigation>
{
    /// <inheritdoc />
    public string Description => "Investigation must be in a non-terminal status.";

    /// <inheritdoc />
    public bool IsSatisfiedBy(Investigation entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return !entity.Status.IsTerminal;
    }
}
