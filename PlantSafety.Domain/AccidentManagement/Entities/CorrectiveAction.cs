// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           CorrectiveAction.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the Accident aggregate representing a single Corrective and
//                  Preventive Action (CAPA) item. Grounded in "Corrective Actions (CAPA):
//                  Implementing solutions to prevent recurrence" (Accident Management) and the
//                  Full Risk Management Process Step 6 requirement that "Each corrective action
//                  must include: Responsible person, Deadline, Verification steps, Evidence of
//                  completion".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Entities;

/// <summary>A single CAPA (Corrective and Preventive Action) item tracked against an Accident investigation.</summary>
public sealed class CorrectiveAction
{
    /// <summary>Unique identity of this corrective action.</summary>
    public CorrectiveActionId CorrectiveActionId { get; private set; }

    /// <summary>Identity of the parent Accident aggregate.</summary>
    public AccidentId AccidentId { get; private set; }

    /// <summary>Classification of the corrective action (Appendix G).</summary>
    public CorrectiveActionCategory Category { get; private set; }

    /// <summary>Description of the action to be taken.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Identity of the person responsible for completing the action.</summary>
    public UserId ResponsiblePersonId { get; private set; }

    /// <summary>Deadline by which the action must be completed.</summary>
    public UtcTimestamp DueAtUtc { get; private set; }

    /// <summary>Current lifecycle status of the action.</summary>
    public CorrectiveActionStatus Status { get; private set; }

    /// <summary>Free-form description of the follow-up verification method used to confirm effectiveness.</summary>
    public string? VerificationSteps { get; private set; }

    /// <summary>Evidence of completion (document reference, photo reference, inspection ID, etc.).</summary>
    public string? CompletionEvidence { get; private set; }

    /// <summary>UTC instant the action was marked Completed, if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>UTC instant the action's effectiveness was verified, if any.</summary>
    public UtcTimestamp? VerifiedAtUtc { get; private set; }

    /// <summary>Identity of the user who verified effectiveness, if any.</summary>
    public UserId? VerifiedBy { get; private set; }

    /// <summary>UTC instant this corrective action was created.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private CorrectiveAction() { }

    /// <summary>Creates a new Open corrective action.</summary>
    public CorrectiveAction(
        AccidentId accidentId,
        CorrectiveActionCategory category,
        string description,
        UserId responsiblePersonId,
        UtcTimestamp dueAtUtc,
        UtcTimestamp createdAtUtc)
    {
        CorrectiveActionId = CorrectiveActionId.New();
        AccidentId = accidentId;
        Category = category;
        Description = (description ?? string.Empty).Trim();
        ResponsiblePersonId = responsiblePersonId;
        DueAtUtc = dueAtUtc;
        Status = CorrectiveActionStatus.Open;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>Transitions the action to InProgress.</summary>
    public void Start()
    {
        if (Status != CorrectiveActionStatus.Open)
            throw new InvalidOperationException($"CorrectiveAction {CorrectiveActionId} must be Open to start; current status is '{Status}'.");
        Status = CorrectiveActionStatus.InProgress;
    }

    /// <summary>Marks the action Completed with supporting evidence.</summary>
    public void Complete(string completionEvidence, UtcTimestamp completedAtUtc)
    {
        if (Status is not (CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress or CorrectiveActionStatus.Overdue))
            throw new InvalidOperationException($"CorrectiveAction {CorrectiveActionId} cannot be completed from status '{Status}'.");

        Status = CorrectiveActionStatus.Completed;
        CompletionEvidence = (completionEvidence ?? string.Empty).Trim();
        CompletedAtUtc = completedAtUtc;
    }

    /// <summary>Records follow-up verification that the completed action is effective (Full Risk Management Process, Step 7).</summary>
    public void VerifyEffective(UserId verifiedBy, string verificationSteps, UtcTimestamp verifiedAtUtc)
    {
        if (Status != CorrectiveActionStatus.Completed)
            throw new InvalidOperationException($"CorrectiveAction {CorrectiveActionId} must be Completed before verification; current status is '{Status}'.");

        Status = CorrectiveActionStatus.VerifiedEffective;
        VerifiedBy = verifiedBy;
        VerificationSteps = (verificationSteps ?? string.Empty).Trim();
        VerifiedAtUtc = verifiedAtUtc;
    }

    /// <summary>Marks the action Overdue when its due date has passed without completion.</summary>
    public void MarkOverdue()
    {
        if (Status is CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress)
            Status = CorrectiveActionStatus.Overdue;
    }
}
