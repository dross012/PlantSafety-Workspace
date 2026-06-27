/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IInvestigationRepository.cs
    DESCRIPTION:        Repository contract for the Investigation aggregate root. Defined in the Domain layer;
                        implemented by Infrastructure. Application layer depends only on this interface.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.AIInvestigations.Aggregates;
using Intelligence.Domain.AIInvestigations.ValueObjects;

namespace Intelligence.Domain.AIInvestigations.Interfaces;

/// <summary>
/// Persistence contract for <see cref="Investigation"/> aggregates.
/// Implemented by <c>Intelligence.Infrastructure</c>; consumed by the Application layer.
/// </summary>
public interface IInvestigationRepository : IRepository<Investigation, InvestigationId>
{
    /// <summary>Returns all open (non-terminal) investigations, ordered by opened date descending.</summary>
    Task<IReadOnlyList<Investigation>> GetOpenInvestigationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all investigations requested by the given actor.</summary>
    Task<IReadOnlyList<Investigation>> GetByActorAsync(string actorId, CancellationToken cancellationToken = default);
}
