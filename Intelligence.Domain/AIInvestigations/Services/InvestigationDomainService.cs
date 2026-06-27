/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationDomainService.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Constants;
using Intelligence.Domain.AIInvestigations.Aggregates;

namespace Intelligence.Domain.AIInvestigations.Services;

/// <summary>
/// Stateless domain service for investigation business rules that span multiple aggregates
/// or require cross-entity computation. No I/O permitted here.
/// </summary>
public sealed class InvestigationDomainService
{
    /// <summary>Returns true when the investigation has enough high-confidence findings to support completion.</summary>
    public bool HasSufficientHighConfidenceFindings(Investigation investigation)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        return investigation.Findings.Any(f => f.Confidence.MeetsMinimumThreshold);
    }

    /// <summary>Computes the average confidence score across all findings. Returns 0 when there are no findings.</summary>
    public double ComputeAverageConfidence(Investigation investigation)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        if (investigation.Findings.Count == 0) return 0.0;
        return investigation.Findings.Average(f => f.Confidence.Value);
    }

    /// <summary>Returns true when adding one more finding would exceed the limit.</summary>
    public bool WouldExceedFindingLimit(Investigation investigation)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        return investigation.Findings.Count >= IntelligenceConstants.MaxFindingsPerInvestigation;
    }
}
