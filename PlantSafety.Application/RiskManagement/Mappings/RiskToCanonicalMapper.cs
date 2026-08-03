// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Application
//  File:           RiskToCanonicalMapper.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Maps the real Risk aggregate to the enterprise canonical model
//                  (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel.CanonicalRiskEventV1
//                  /V2/V3) for enterprise integration consumers (ERP/MES/CMMS adapters, event
//                  mesh). Lives in this module's Application layer (not SharedKernel) because
//                  SharedKernel must never reference module Domain/Application types. V3's
//                  forward-looking fields are deterministic projections of the real aggregate's
//                  own NextReviewDueUtc/review-workflow state -- no genuine ML predictor exists for
//                  risk-event recurrence in the Phase 5 predictor infrastructure, so nothing is
//                  fabricated. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

namespace PlantSafety.Application.RiskManagement.Mappings;

/// <summary>Maps a real <see cref="Risk"/> aggregate to the enterprise canonical risk-event shapes.</summary>
public static class RiskToCanonicalMapper
{
    /// <summary>Maps <paramref name="risk"/> to its V1 (stable) canonical projection.</summary>
    public static CanonicalRiskEventV1 ToCanonicalV1(this Risk risk)
    {
        ArgumentNullException.ThrowIfNull(risk);

        return new CanonicalRiskEventV1
        {
            RiskEventId = risk.RiskId.Value,
            TenantId = risk.TenantId,
            SiteId = risk.SiteId,
            RiskTitle = risk.RiskTitle,
            Category = risk.Category.Code,
            Location = risk.Location,
            HazardSource = risk.HazardSource,
            Status = risk.Status.ToString(),
            CreatedAtUtc = risk.CreatedAtUtc,
        };
    }

    /// <summary>Maps <paramref name="risk"/> to its V2 (extended) canonical projection.</summary>
    public static CanonicalRiskEventV2 ToCanonicalV2(this Risk risk)
    {
        ArgumentNullException.ThrowIfNull(risk);

        return new CanonicalRiskEventV2
        {
            RiskEventId = risk.RiskId.Value,
            TenantId = risk.TenantId,
            SiteId = risk.SiteId,
            RiskTitle = risk.RiskTitle,
            Category = risk.Category.Code,
            Location = risk.Location,
            HazardSource = risk.HazardSource,
            Status = risk.Status.ToString(),
            CreatedAtUtc = risk.CreatedAtUtc,
            Priority = risk.Priority?.Code,
            CurrentScoreSeverity = risk.CurrentScore?.Severity,
            CurrentScoreLikelihood = risk.CurrentScore?.Likelihood,
            ResidualScoreSeverity = risk.ResidualScore?.Severity,
            ResidualScoreLikelihood = risk.ResidualScore?.Likelihood,
            ControlMeasureCount = risk.ControlMeasures.Count,
            ReviewCadenceCode = risk.ReviewCadence.Code,
            LastReviewedAtUtc = risk.LastReviewedAtUtc,
        };
    }

    /// <summary>
    /// Maps <paramref name="risk"/> to its V3 (forward-looking) canonical projection.
    /// <paramref name="nowUtc"/> is the instant against which <see cref="Risk.NextReviewDueUtc"/>
    /// is compared to derive <see cref="CanonicalRiskEventV3.IsReviewOverdue"/> -- a deterministic
    /// projection, not an ML prediction.
    /// </summary>
    public static CanonicalRiskEventV3 ToCanonicalV3(this Risk risk, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(risk);

        return new CanonicalRiskEventV3
        {
            RiskEventId = risk.RiskId.Value,
            TenantId = risk.TenantId,
            SiteId = risk.SiteId,
            RiskTitle = risk.RiskTitle,
            Category = risk.Category.Code,
            Location = risk.Location,
            HazardSource = risk.HazardSource,
            Status = risk.Status.ToString(),
            CreatedAtUtc = risk.CreatedAtUtc,
            Priority = risk.Priority?.Code,
            CurrentScoreSeverity = risk.CurrentScore?.Severity,
            CurrentScoreLikelihood = risk.CurrentScore?.Likelihood,
            ResidualScoreSeverity = risk.ResidualScore?.Severity,
            ResidualScoreLikelihood = risk.ResidualScore?.Likelihood,
            ControlMeasureCount = risk.ControlMeasures.Count,
            ReviewCadenceCode = risk.ReviewCadence.Code,
            LastReviewedAtUtc = risk.LastReviewedAtUtc,
            NextReviewDueUtc = risk.NextReviewDueUtc,
            IsReviewOverdue = risk.NextReviewDueUtc.HasValue && risk.NextReviewDueUtc.Value < nowUtc,
        };
    }
}
