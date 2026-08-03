// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReport.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single printable/exportable audit report (Handbook
//                  Phase 5 §6.2): a point-in-time compliance-formatted export of a target
//                  entity's audit state (identity, timeline, approvals, exceptions, signatures)
//                  in PDF, CSV, or JSON form. The aggregate owns section composition and
//                  lifecycle (Draft -> Generated | Failed); the Application layer's
//                  IAuditReportRenderer performs the actual byte-stream rendering and reports the
//                  outcome back via MarkGenerated/MarkFailed. Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Entities;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.Events;
using PlantSafety.Domain.Auditing.AuditReport.Exceptions;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Aggregates;

/// <summary>Aggregate root for a single printable/exportable audit report.</summary>
public sealed class AuditReport
{
    // ---- Identity --------------------------------------------------------------

    /// <summary>Strong-typed identity of this report.</summary>
    public AuditReportId AuditReportId { get; private set; }

    /// <summary>Tenant that owns this report.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site scope, or null for tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Type discriminator of the entity this report documents.</summary>
    public string TargetEntityType { get; private set; } = string.Empty;

    /// <summary>Identity of the entity this report documents.</summary>
    public Guid TargetEntityId { get; private set; }

    /// <summary>Requested output format.</summary>
    public AuditReportFormat Format { get; private set; } = AuditReportFormat.Default;

    /// <summary>Current lifecycle status.</summary>
    public AuditReportStatus Status { get; private set; } = AuditReportStatus.Default;

    private readonly List<AuditReportSection> _sections = [];

    /// <summary>Composed sections, in canonical print order.</summary>
    public IReadOnlyList<AuditReportSection> Sections => _sections.AsReadOnly();

    /// <summary>Content length of the rendered output, once generated.</summary>
    public long? ContentSizeBytes { get; private set; }

    /// <summary>Storage reference (e.g. blob URI or content hash) for the rendered output, once generated.</summary>
    public string? RenderedContentReference { get; private set; }

    /// <summary>Reason generation failed, if <see cref="Status"/> is Failed.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>Identity of the user or system actor who requested this report.</summary>
    public UserId RequestedBy { get; private set; }

    /// <summary>UTC instant the report was requested/drafted.</summary>
    public UtcTimestamp RequestedAtUtc { get; private set; }

    /// <summary>UTC instant the report finished generating (success or failure), if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------------

    private readonly List<IAuditReportDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAuditReportDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAuditReportDomainEvent>(_domainEvents);

    // For rehydration only.
    private AuditReport() { }

    /// <summary>Drafts a new AuditReport with the given composed sections.</summary>
    public static AuditReport Draft(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        AuditReportFormat format, UserId requestedBy, UtcTimestamp requestedAtUtc,
        IEnumerable<AuditReportSection> sections)
        => Draft(tenantId, siteId, targetEntityType, targetEntityId, format, requestedBy, requestedAtUtc, _ => sections);

    /// <summary>
    /// Drafts a new AuditReport, deferring section construction to <paramref name="sectionsFactory"/>
    /// so callers that need each <see cref="AuditReportSection"/> to carry this report's own
    /// freshly-generated <see cref="AuditReportId"/> (e.g. <c>AuditReportContentBuilder</c>) can do
    /// so without a two-phase construction dance.
    /// </summary>
    public static AuditReport Draft(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        AuditReportFormat format, UserId requestedBy, UtcTimestamp requestedAtUtc,
        Func<AuditReportId, IEnumerable<AuditReportSection>> sectionsFactory)
    {
        var report = new AuditReport
        {
            AuditReportId = AuditReportId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            TargetEntityType = (targetEntityType ?? string.Empty).Trim(),
            TargetEntityId = targetEntityId,
            Format = format,
            Status = AuditReportStatus.Draft,
            RequestedBy = requestedBy,
            RequestedAtUtc = requestedAtUtc,
        };

        report._sections.AddRange(sectionsFactory(report.AuditReportId).OrderBy(s => s.SectionType.PrintOrder));

        return report;
    }

    /// <summary>Marks the report as successfully rendered and raises <see cref="AuditReportGeneratedEvent"/>.</summary>
    public void MarkGenerated(
        string renderedContentReference, long contentSizeBytes, UtcTimestamp completedAtUtc,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != AuditReportStatus.Draft)
            throw new AuditReportInvalidStateException(AuditReportId, Status.Code, "MarkGenerated", correlationId, causationId);

        Status = AuditReportStatus.Generated;
        RenderedContentReference = renderedContentReference;
        ContentSizeBytes = contentSizeBytes;
        CompletedAtUtc = completedAtUtc;

        AddDomainEvent(new AuditReportGeneratedEvent(
            Uuid7.New(), AuditReportId, TargetEntityType, TargetEntityId, Format, contentSizeBytes,
            completedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Marks the report as failed and raises <see cref="AuditReportGenerationFailedEvent"/>.</summary>
    public void MarkFailed(
        string reason, UtcTimestamp failedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != AuditReportStatus.Draft)
            throw new AuditReportInvalidStateException(AuditReportId, Status.Code, "MarkFailed", correlationId, causationId);

        Status = AuditReportStatus.Failed;
        FailureReason = (reason ?? string.Empty).Trim();
        CompletedAtUtc = failedAtUtc;

        AddDomainEvent(new AuditReportGenerationFailedEvent(
            Uuid7.New(), AuditReportId, Format, FailureReason, failedAtUtc, actor, correlationId, causationId));
    }

    private void AddDomainEvent(IAuditReportDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
