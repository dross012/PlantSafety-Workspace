// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReport.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a generated ESG disclosure report (GHG Protocol / CSRD / SEC
//                  climate-disclosure-style reporting structures), assembled from an
//                  AdvancedMetricsLedger's period-end data. Data-governance rule: once Published, a
//                  report is immutable; corrections are made only by generating a new report that
//                  Supersedes it (Version increments, SupersedesReportId links the chain).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;

/// <summary>Aggregate root for a generated, versioned ESG disclosure report.</summary>
public sealed class EsgDisclosureReport
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this report aggregate.</summary>
    public EsgDisclosureReportId EsgDisclosureReportId { get; private set; }

    /// <summary>Tenant that owns this report.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this report is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Identity of the source AdvancedMetricsLedger this report was generated from.</summary>
    public AdvancedMetricsLedgerId SourceLedgerId { get; private set; }

    /// <summary>Disclosure framework(s) this report is structured to support.</summary>
    public EsgDisclosureFramework Frameworks { get; private set; }

    /// <summary>Start of the reporting period this report covers.</summary>
    public UtcTimestamp ReportingPeriodStartUtc { get; private set; }

    /// <summary>End of the reporting period this report covers.</summary>
    public UtcTimestamp ReportingPeriodEndUtc { get; private set; }

    /// <summary>Version number of this report, starting at 1. Incremented on each Supersede chain link.</summary>
    public int Version { get; private set; }

    /// <summary>Identity of the report this version supersedes, if any.</summary>
    public EsgDisclosureReportId? SupersedesReportId { get; private set; }

    /// <summary>Version identifier of the metric calculation methodology in force when this report was generated.</summary>
    public string MethodologyVersion { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public EsgDisclosureReportStatus Status { get; private set; }

    /// <summary>UTC instant the report was generated (drafted).</summary>
    public UtcTimestamp GeneratedAtUtc { get; private set; }

    /// <summary>Identity of the user who generated this report.</summary>
    public UserId GeneratedBy { get; private set; }

    /// <summary>UTC instant the report was published, if it has been.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAdvancedMetricsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAdvancedMetricsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAdvancedMetricsDomainEvent>(_domainEvents);

    // For rehydration only.
    private EsgDisclosureReport() { }

    // ---- Factory: Generate (Draft) -----------------------------------------------------------

    /// <summary>Generates a new (version-1) draft ESG disclosure report from the given source ledger.</summary>
    public static EsgDisclosureReport Generate(
        UserId generatedBy,
        UtcTimestamp generatedAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        AdvancedMetricsLedgerId sourceLedgerId,
        EsgDisclosureFramework frameworks,
        UtcTimestamp reportingPeriodStartUtc,
        UtcTimestamp reportingPeriodEndUtc,
        string methodologyVersion)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));
        if (frameworks == EsgDisclosureFramework.None)
            throw new ArgumentException("At least one disclosure framework must be selected.", nameof(frameworks));
        if (string.IsNullOrWhiteSpace(methodologyVersion))
            throw new ArgumentException("MethodologyVersion must not be null or whitespace.", nameof(methodologyVersion));

        var report = new EsgDisclosureReport
        {
            EsgDisclosureReportId    = EsgDisclosureReportId.New(),
            TenantId                  = tenantId,
            SiteId                    = siteId,
            SourceLedgerId            = sourceLedgerId,
            Frameworks                = frameworks,
            ReportingPeriodStartUtc   = reportingPeriodStartUtc,
            ReportingPeriodEndUtc     = reportingPeriodEndUtc,
            Version                   = 1,
            SupersedesReportId        = null,
            MethodologyVersion        = methodologyVersion.Trim(),
            Status                    = EsgDisclosureReportStatus.Draft,
            GeneratedAtUtc            = generatedAtUtc,
            GeneratedBy               = generatedBy,
        };

        report.AddDomainEvent(new EsgDisclosureReportGeneratedEvent(
            AdvancedMetricsEventId.New(), report.EsgDisclosureReportId, report.Version, frameworks,
            generatedAtUtc, actor, correlationId, causationId));

        return report;
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Publishes the draft report, after which it becomes immutable.</summary>
    public void Publish(UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!EsgDisclosureReportLifecycleRules.CanPublish(Status))
            throw new EsgDisclosureReportInvalidStatusTransitionException(
                EsgDisclosureReportId, Status.ToString(), EsgDisclosureReportStatus.Published.ToString(), correlationId, causationId);

        Status = EsgDisclosureReportStatus.Published;
        PublishedAtUtc = publishedAtUtc;

        AddDomainEvent(new EsgDisclosureReportPublishedEvent(
            AdvancedMetricsEventId.New(), EsgDisclosureReportId, Version, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: CreateSupersedingVersion -----------------------------------------------------------

    /// <summary>
    /// Creates the next-version draft report superseding this (published) report. This is the only
    /// permitted correction path for a published report -- the published row itself is never
    /// mutated. Marks this report as Superseded and raises <see cref="EsgDisclosureReportSupersededEvent"/>.
    /// </summary>
    public EsgDisclosureReport CreateSupersedingVersion(
        UserId generatedBy,
        UtcTimestamp generatedAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        AdvancedMetricsLedgerId sourceLedgerId,
        string methodologyVersion)
    {
        if (!EsgDisclosureReportLifecycleRules.CanSupersede(Status))
            throw new EsgDisclosureReportInvalidStatusTransitionException(
                EsgDisclosureReportId, Status.ToString(), EsgDisclosureReportStatus.Superseded.ToString(), correlationId, causationId);

        var next = new EsgDisclosureReport
        {
            EsgDisclosureReportId    = EsgDisclosureReportId.New(),
            TenantId                  = TenantId,
            SiteId                    = SiteId,
            SourceLedgerId            = sourceLedgerId,
            Frameworks                = Frameworks,
            ReportingPeriodStartUtc   = ReportingPeriodStartUtc,
            ReportingPeriodEndUtc     = ReportingPeriodEndUtc,
            Version                   = Version + 1,
            SupersedesReportId        = EsgDisclosureReportId,
            MethodologyVersion        = (methodologyVersion ?? MethodologyVersion).Trim(),
            Status                    = EsgDisclosureReportStatus.Draft,
            GeneratedAtUtc            = generatedAtUtc,
            GeneratedBy               = generatedBy,
        };

        next.AddDomainEvent(new EsgDisclosureReportGeneratedEvent(
            AdvancedMetricsEventId.New(), next.EsgDisclosureReportId, next.Version, next.Frameworks,
            generatedAtUtc, actor, correlationId, causationId));

        Status = EsgDisclosureReportStatus.Superseded;

        AddDomainEvent(new EsgDisclosureReportSupersededEvent(
            AdvancedMetricsEventId.New(), EsgDisclosureReportId, next.EsgDisclosureReportId,
            generatedAtUtc, actor, correlationId, causationId));

        return next;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAdvancedMetricsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
