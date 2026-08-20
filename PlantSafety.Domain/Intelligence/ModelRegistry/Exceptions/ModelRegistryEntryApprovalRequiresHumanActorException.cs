// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryApprovalRequiresHumanActorException.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL (Safety Rule 4.5): Thrown when a caller attempts to approve a
//                  ModelRegistryEntry using a system or AI-agent actor instead of a human user.
//                  ApprovedByActorId must always be a human actor -- never a system identity. Ported
//                  concept from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/Entities/ModelRegistryEntry.cs's documented
//                  invariant ("Approval requires an explicit human approver actor ID"), which the
//                  orphan itself never actually enforced in code.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Exceptions;

/// <summary>SAFETY-CRITICAL: Thrown when model approval is attempted by a non-human actor (Safety Rule 4.5).</summary>
public sealed class ModelRegistryEntryApprovalRequiresHumanActorException : ModelRegistryEntryDomainException
{
    /// <summary>Creates the exception for the given entry identity.</summary>
    public ModelRegistryEntryApprovalRequiresHumanActorException(
        ModelRegistryEntryId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"ModelRegistryEntry '{id}' cannot be approved: approval requires an explicit human approver actor (Safety Rule 4.5).",
            "ModelRegistry.ApprovalRequiresHumanActor",
            "SafetyRule",
            "Critical",
            correlationId,
            causationId)
    {
    }
}
