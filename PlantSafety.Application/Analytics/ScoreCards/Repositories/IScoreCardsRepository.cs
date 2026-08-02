// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           IScoreCardsRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the ScoreCards read-model. Implemented in
//                  the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Analytics.ScoreCards.Interfaces.IScoreCardsRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Permits.Repositories.
//                  IPermitsRepository's identical precedent for CQRS read/write repository
//                  separation. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.ScoreCards.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Repositories;

/// <summary>Read-only repository for ScoreCards data. Implemented in the Infrastructure layer.</summary>
public interface IScoreCardsRepository
{
    /// <summary>
    /// Returns the ScoreCards read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the ScoreCards record belongs to a different tenant.
    /// </summary>
    Task<ScoreCardsItemDto?> GetByIdAsync(Guid scoreCardsId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns ScoreCards items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ScoreCardsItemDto>> GetScoreCardsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
