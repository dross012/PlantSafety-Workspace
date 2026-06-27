/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ISafetyPredictionRepository.cs
    DESCRIPTION:        SAFETY-CRITICAL repository contract for the SafetyPrediction aggregate root.
                        Queries for unacknowledged high-risk predictions must be fast — these drive
                        human escalation workflows where latency directly affects safety outcomes.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.PredictiveSafety.Aggregates;
using Intelligence.Domain.PredictiveSafety.ValueObjects;

namespace Intelligence.Domain.PredictiveSafety.Interfaces;

/// <summary>
/// SAFETY-CRITICAL: Persistence contract for <see cref="SafetyPrediction"/> aggregates.
/// High-risk unacknowledged query must be optimised — latency here affects safety escalation.
/// </summary>
public interface ISafetyPredictionRepository : IRepository<SafetyPrediction, SafetyPredictionId>
{
    /// <summary>
    /// Returns all unacknowledged predictions at High or Critical risk level that meet the
    /// minimum confidence threshold, ordered by risk level descending then generated date ascending.
    /// </summary>
    Task<IReadOnlyList<SafetyPrediction>> GetUnacknowledgedHighRiskAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all unacknowledged predictions for the given subject (exact match).</summary>
    Task<IReadOnlyList<SafetyPrediction>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);
}
