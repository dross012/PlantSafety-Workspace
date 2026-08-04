// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessment.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the CommunityImpactAssessment bounded context (Community
//                  Impact Assessment subdomain of CommunitySafety). Records the assessed impact
//                  of a specific on-site SafetyEvents incident on the surrounding community. The
//                  incident is referenced only via a weak Guid (IncidentId), never a direct type
//                  reference, per the Canonical Adapter Rule / Zero Cross-Module Leakage rule --
//                  this is the module's SafetyEvents integration point. A Severe assessment is
//                  also the Compliance integration point: downstream EPCRA/community
//                  right-to-know reporting consumes CommunityImpactAssessedEvent. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Events;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Exceptions;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Rules;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Aggregates;

/// <summary>Aggregate root for the CommunityImpactAssessment bounded context.</summary>
public sealed partial class CommunityImpactAssessment
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CommunityImpactAssessment aggregate.</summary>
    public CommunityImpactAssessmentId CommunityImpactAssessmentId { get; private set; }

    /// <summary>Tenant that owns this assessment record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the assessment relates to.</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CommunityImpactAssessmentStatus Status { get; private set; }

    // ---- Assessment-specific business properties -------------------------------

    /// <summary>
    /// Weak reference (Guid only) to the on-site SafetyEvents incident this assessment relates
    /// to. This is the CommunitySafety-to-SafetyEvents integration point.
    /// </summary>
    public Guid IncidentId { get; private set; }

    /// <summary>Assessed severity of the community impact.</summary>
    public CommunityImpactSeverity Severity { get; private set; }

    /// <summary>Geographic/community area affected.</summary>
    public string AffectedArea { get; private set; } = string.Empty;

    /// <summary>Estimated number of community members affected, if known.</summary>
    public int? EstimatedPersonsAffected { get; private set; }

    /// <summary>Narrative description of the assessed impact.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Whether this assessment triggers EPCRA/community right-to-know reporting obligations (Compliance integration hook).</summary>
    public bool RequiresRightToKnowReporting { get; private set; }

    /// <summary>Identity of the user who performed the assessment.</summary>
    public UserId AssessedByUserId { get; private set; }

    /// <summary>UTC instant the assessment was recorded.</summary>
    public UtcTimestamp AssessedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant the assessment was finalized, once finalized.</summary>
    public UtcTimestamp? FinalizedAtUtc { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICommunityImpactAssessmentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICommunityImpactAssessmentDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICommunityImpactAssessmentDomainEvent>(_domainEvents);

    // For rehydration only.
    private CommunityImpactAssessment() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Records a new CommunityImpactAssessment aggregate and raises
    /// <see cref="CommunityImpactAssessedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static CommunityImpactAssessment Record(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        Guid incidentId,
        CommunityImpactSeverity severity,
        string affectedArea,
        string description,
        UserId assessedByUserId,
        int? estimatedPersonsAffected = null)
    {
        var normalizedAffectedArea = (affectedArea ?? string.Empty).Trim();
        var normalizedDescription = (description ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId is required.", nameof(siteId));
        if (incidentId == Guid.Empty)
            throw new ArgumentException("IncidentId is required.", nameof(incidentId));
        if (string.IsNullOrWhiteSpace(normalizedAffectedArea))
            throw new ArgumentException("AffectedArea is required.", nameof(affectedArea));
        if (string.IsNullOrWhiteSpace(normalizedDescription))
            throw new ArgumentException("Description is required.", nameof(description));

        var agg = new CommunityImpactAssessment
        {
            CommunityImpactAssessmentId = CommunityImpactAssessmentId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = CommunityImpactAssessmentStatus.Recorded,
            IncidentId = incidentId,
            Severity = severity,
            AffectedArea = normalizedAffectedArea,
            EstimatedPersonsAffected = estimatedPersonsAffected,
            Description = normalizedDescription,
            RequiresRightToKnowReporting = severity == CommunityImpactSeverity.Severe,
            AssessedByUserId = assessedByUserId,
            AssessedAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new CommunityImpactAssessedEvent(
            CommunityImpactAssessmentEventId.New(), agg.CommunityImpactAssessmentId, agg.IncidentId,
            agg.Severity, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Reconstructs a CommunityImpactAssessment aggregate from previously persisted state, without raising domain events.</summary>
    public static CommunityImpactAssessment Rehydrate(
        CommunityImpactAssessmentId communityImpactAssessmentId,
        Guid tenantId,
        Guid siteId,
        CommunityImpactAssessmentStatus status,
        Guid incidentId,
        CommunityImpactSeverity severity,
        string affectedArea,
        int? estimatedPersonsAffected,
        string description,
        bool requiresRightToKnowReporting,
        UserId assessedByUserId,
        UtcTimestamp assessedAtUtc,
        UtcTimestamp createdAtUtc,
        UserId createdBy,
        UtcTimestamp? finalizedAtUtc)
        => new()
        {
            CommunityImpactAssessmentId = communityImpactAssessmentId,
            TenantId = tenantId,
            SiteId = siteId,
            Status = status,
            IncidentId = incidentId,
            Severity = severity,
            AffectedArea = affectedArea,
            EstimatedPersonsAffected = estimatedPersonsAffected,
            Description = description,
            RequiresRightToKnowReporting = requiresRightToKnowReporting,
            AssessedByUserId = assessedByUserId,
            AssessedAtUtc = assessedAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            FinalizedAtUtc = finalizedAtUtc,
        };

    // ---- Lifecycle: Finalize ---------------------------------------------------

    /// <summary>Finalizes the assessment, making it no longer revisable.</summary>
    /// <exception cref="CommunityImpactAssessmentInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Finalize(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityImpactAssessmentLifecycleRules.CanTransition(Status, CommunityImpactAssessmentStatus.Finalized))
            throw new CommunityImpactAssessmentInvalidStatusTransitionException(
                CommunityImpactAssessmentId, Status.ToString(), CommunityImpactAssessmentStatus.Finalized.ToString(), correlationId, causationId);

        Status = CommunityImpactAssessmentStatus.Finalized;
        FinalizedAtUtc = nowUtc;
    }

    // ---- Lifecycle: Archive ---------------------------------------------------

    /// <summary>Archives (soft-deletes) a finalized assessment.</summary>
    /// <exception cref="CommunityImpactAssessmentInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityImpactAssessmentLifecycleRules.CanTransition(Status, CommunityImpactAssessmentStatus.Archived))
            throw new CommunityImpactAssessmentInvalidStatusTransitionException(
                CommunityImpactAssessmentId, Status.ToString(), CommunityImpactAssessmentStatus.Archived.ToString(), correlationId, causationId);

        Status = CommunityImpactAssessmentStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICommunityImpactAssessmentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
