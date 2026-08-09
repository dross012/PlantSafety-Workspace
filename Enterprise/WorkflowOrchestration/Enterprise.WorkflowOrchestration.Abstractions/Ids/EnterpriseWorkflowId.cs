// ============================================================================
//  Project:        Enterprise.WorkflowOrchestration
//  Module:         Abstractions.Ids
//  File:           EnterpriseWorkflowId.cs
//  Created:        2026-08-08
//  Description:    Phase D.6 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): strongly typed identifier for an
//                  orchestrated enterprise workflow instance. Distinct
//                  from Shared.WorkflowEngine's per-event WorkflowDecision
//                  (Path C.7) -- this identifies a coordinated, stateful
//                  orchestration run across multiple triggers, not a
//                  single deterministic evaluation.
// ============================================================================
namespace Enterprise.WorkflowOrchestration.Abstractions.Ids;

/// <summary>Strongly typed identifier for an orchestrated enterprise workflow instance. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct EnterpriseWorkflowId
{
    private EnterpriseWorkflowId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="EnterpriseWorkflowId"/>.</summary>
    public static EnterpriseWorkflowId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as an <see cref="EnterpriseWorkflowId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static EnterpriseWorkflowId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("EnterpriseWorkflowId cannot be Guid.Empty.", nameof(value));
        }

        return new EnterpriseWorkflowId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
