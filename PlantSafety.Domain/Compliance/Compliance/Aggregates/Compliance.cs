// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           Compliance.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  regulatory-governance record shape (regulation reference, rule
//                  description, score, non-conformances) grounded in
//                  project_plantsafety_domains.md Module 5 (Compliance: "Regulatory
//                  governance, rule evaluation, and non-conformance workflows").
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Compliance bounded context. Tracks a single
//                  regulatory-compliance evaluation record: the regulation/rule it
//                  evaluates, its computed score, and non-conformance volume.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Aggregates;

/// <summary>Aggregate root for the Compliance bounded context.</summary>
public sealed partial class Compliance
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Compliance aggregate.</summary>
    public ComplianceId ComplianceId { get; private set; }

    /// <summary>Tenant that owns this compliance record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this compliance record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ComplianceStatus Status { get; private set; }

    // ---- Compliance-specific business properties ----------------------------

    /// <summary>Regulation/standard reference this record evaluates (e.g., "29 CFR 1910.147").</summary>
    public string RegulationReference { get; private set; } = string.Empty;

    /// <summary>Narrative description of the rule being evaluated.</summary>
    public string RuleDescription { get; private set; } = string.Empty;

    /// <summary>Computed compliance score (0-100) for this rule/regulation.</summary>
    public decimal ComplianceScore { get; private set; }

    /// <summary>Count of open non-conformances tied to this compliance record.</summary>
    public int NonConformanceCount { get; private set; }

    /// <summary>Identity of the party accountable for maintaining compliance.</summary>
    public UserId ResponsiblePartyId { get; private set; }

    /// <summary>UTC timestamp this compliance record was last evaluated.</summary>
    public UtcTimestamp? LastEvaluatedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IComplianceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IComplianceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IComplianceDomainEvent>(_domainEvents);

    // For rehydration only.
    private Compliance() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Compliance aggregate and raises <see cref="ComplianceCreatedEvent"/>.</summary>
    public static Compliance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string regulationReference = "",
        string ruleDescription = "",
        decimal complianceScore = 0m,
        int nonConformanceCount = 0,
        UserId responsiblePartyId = default,
        UtcTimestamp? lastEvaluatedAtUtc = null)
    {
        var agg = new Compliance
        {
            ComplianceId          = ComplianceId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = ComplianceStatus.Created,
            RegulationReference   = (regulationReference ?? string.Empty).Trim(),
            RuleDescription       = (ruleDescription ?? string.Empty).Trim(),
            ComplianceScore       = complianceScore,
            NonConformanceCount   = nonConformanceCount,
            ResponsiblePartyId    = responsiblePartyId,
            LastEvaluatedAtUtc    = lastEvaluatedAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new ComplianceCreatedEvent(
            EventId:       new ComplianceEventId(Uuid7.New()),
            ComplianceId:      agg.ComplianceId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IComplianceDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ComplianceStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Compliance {ComplianceId}.");
    }
}
