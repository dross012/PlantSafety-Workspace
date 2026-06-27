/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationFinding.cs
    DESCRIPTION:        Entity representing a single finding within an AI investigation. Owned by the
                        Investigation aggregate. Requires a non-empty description and explanation to
                        satisfy explainability requirements (Safety Rule 4.6). Immutable once created.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.AIInvestigations.ValueObjects;

namespace Intelligence.Domain.AIInvestigations.Entities;

/// <summary>A single finding within an AI investigation. Immutable after creation.</summary>
public sealed class InvestigationFinding : Entity<Guid>
{
    /// <summary>The investigation this finding belongs to.</summary>
    public InvestigationId InvestigationId { get; private init; }

    /// <summary>Human-readable description of the finding (Safety Rule 4.6).</summary>
    public string Description { get; private init; }

    /// <summary>The confidence level of this finding.</summary>
    public FindingConfidence Confidence { get; private init; }

    /// <summary>Human-readable explanation of why this finding was raised (Safety Rule 4.6).</summary>
    public string Explanation { get; private init; }

    /// <summary>The actor (model or human) that produced this finding.</summary>
    public string ActorId { get; private init; }

    /// <summary>UTC timestamp when this finding was recorded.</summary>
    public DateTime RecordedAtUtc { get; private init; }

    private InvestigationFinding(
        Guid id,
        InvestigationId investigationId,
        string description,
        FindingConfidence confidence,
        string explanation,
        string actorId)
        : base(id)
    {
        InvestigationId = investigationId;
        Description = description;
        Confidence = confidence;
        Explanation = explanation;
        ActorId = actorId;
        RecordedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Creates a new <see cref="InvestigationFinding"/>.</summary>
    /// <param name="investigationId">The owning investigation.</param>
    /// <param name="description">Finding description. Required (Safety Rule 4.6).</param>
    /// <param name="confidence">The confidence level of this finding.</param>
    /// <param name="explanation">Explanation of why this finding was raised. Required (Safety Rule 4.6).</param>
    /// <param name="actorId">The producing actor ID.</param>
    public static InvestigationFinding Create(
        InvestigationId investigationId,
        string description,
        FindingConfidence confidence,
        string explanation,
        string actorId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Finding description is required (Safety Rule 4.6).", nameof(description));
        if (string.IsNullOrWhiteSpace(explanation))
            throw new ArgumentException("Finding explanation is required (Safety Rule 4.6).", nameof(explanation));

        return new InvestigationFinding(
            Guid.NewGuid(), investigationId, description.Trim(), confidence, explanation.Trim(), actorId);
    }
}
