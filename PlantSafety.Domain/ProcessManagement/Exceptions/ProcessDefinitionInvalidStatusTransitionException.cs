// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ProcessDefinition lifecycle transition is not permitted.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Exceptions;

/// <summary>Thrown when a <see cref="ProcessDefinition"/> lifecycle transition is not permitted.</summary>
public sealed class ProcessDefinitionInvalidStatusTransitionException : ProcessDefinitionDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ProcessDefinitionInvalidStatusTransitionException(
        ProcessDefinitionId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'ProcessDefinition' {id} from '{from}' to '{to}'.",
            "ProcessManagement.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
