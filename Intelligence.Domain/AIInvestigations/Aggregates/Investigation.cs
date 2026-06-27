/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               Investigation.cs
    DESCRIPTION:        Aggregate Root for the AIInvestigations module. Manages the lifecycle of an AI-driven
                        safety investigation from opening through completion. Enforces all investigation
                        invariants and raises domain events for every state transition.

    INVARIANTS:
        - Terminal investigations cannot receive new findings.
        - Findings cannot exceed MaxFindingsPerInvestigation.
        - Completion requires at least one finding.
        - Terminal investigations cannot be completed again.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;
using Intelligence.Domain.Shared.Exceptions;
using Intelligence.Domain.AIInvestigations.Entities;
using Intelligence.Domain.AIInvestigations.Events;
using Intelligence.Domain.AIInvestigations.ValueObjects;

namespace Intelligence.Domain.AIInvestigations.Aggregates;

/// <summary>Aggregate root for an AI-driven safety investigation.</summary>
public sealed class Investigation : AggregateRoot<InvestigationId>
{
    private readonly List<InvestigationFinding> _findings = new();

    /// <summary>The subject this investigation concerns.</summary>
    public InvestigationSubject Subject { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public InvestigationStatus Status { get; private set; }

    /// <summary>UTC timestamp when this investigation was opened.</summary>
    public DateTime OpenedAtUtc { get; private init; }

    /// <summary>UTC timestamp when this investigation reached a terminal state.</summary>
    public DateTime? ClosedAtUtc { get; private set; }

    /// <summary>The actor who requested this investigation.</summary>
    public string RequestedByActorId { get; private init; }

    /// <summary>The reviewer assigned to complete this investigation.</summary>
    public string? AssignedReviewerActorId { get; private set; }

    /// <summary>All findings recorded against this investigation.</summary>
    public IReadOnlyList<InvestigationFinding> Findings => _findings.AsReadOnly();

    private Investigation(
        InvestigationId id,
        InvestigationSubject subject,
        string requestedByActorId)
        : base(id)
    {
        Subject = subject;
        Status = InvestigationStatus.Open;
        RequestedByActorId = requestedByActorId;
        OpenedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Opens a new investigation.</summary>
    public static Investigation Open(
        InvestigationSubject subject,
        string requestedByActorId,
        Guid correlationId,
        Guid causationId)
    {
        var id = InvestigationId.New();
        var investigation = new Investigation(id, subject, requestedByActorId);

        investigation.RaiseDomainEvent(new InvestigationStartedEvent
        {
            AggregateId = id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            Subject = subject.Value,
            RequestedByActorId = requestedByActorId,
            ActorId = requestedByActorId
        });

        return investigation;
    }

    /// <summary>Adds a finding to this investigation.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when terminal or at finding limit.</exception>
    public void AddFinding(
        string description,
        FindingConfidence confidence,
        string explanation,
        string actorId,
        Guid correlationId,
        Guid causationId)
    {
        if (Status.IsTerminal)
            throw new IntelligenceDomainException(
                "INVESTIGATION_TERMINAL",
                "Cannot add findings to a terminal investigation.",
                correlationId, causationId);

        if (_findings.Count >= IntelligenceConstants.MaxFindingsPerInvestigation)
            throw new IntelligenceDomainException(
                "INVESTIGATION_FINDING_LIMIT_EXCEEDED",
                $"An investigation cannot have more than {IntelligenceConstants.MaxFindingsPerInvestigation} findings.",
                correlationId, causationId);

        var finding = InvestigationFinding.Create(Id, description, confidence, explanation, actorId);
        _findings.Add(finding);

        if (Status == InvestigationStatus.Open)
            Status = InvestigationStatus.InProgress;

        RaiseDomainEvent(new FindingAddedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            FindingId = finding.Id,
            ConfidenceScore = confidence.Value,
            MeetsThreshold = confidence.MeetsMinimumThreshold,
            ActorId = actorId
        });
    }

    /// <summary>Completes the investigation after human review.</summary>
    /// <exception cref="IntelligenceDomainException">Thrown when already terminal or no findings exist.</exception>
    public void Complete(string reviewerActorId, Guid correlationId, Guid causationId)
    {
        if (Status.IsTerminal)
            throw new IntelligenceDomainException(
                "INVESTIGATION_ALREADY_TERMINAL",
                "A terminal investigation cannot be completed.",
                correlationId, causationId);

        if (_findings.Count == 0)
            throw new IntelligenceDomainException(
                "INVESTIGATION_NO_FINDINGS",
                "An investigation must have at least one finding before it can be completed.",
                correlationId, causationId);

        Status = InvestigationStatus.Completed;
        AssignedReviewerActorId = reviewerActorId;
        ClosedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new InvestigationCompletedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            FindingCount = _findings.Count,
            ReviewerActorId = reviewerActorId,
            CompletedAtUtc = ClosedAtUtc.Value,
            ActorId = reviewerActorId
        });
    }
}
