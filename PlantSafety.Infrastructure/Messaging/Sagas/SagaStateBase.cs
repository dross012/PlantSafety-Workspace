// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging
//  Layer:          Infrastructure
//  File:           SagaStateBase.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for MassTransit saga state machines.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Sagas;

/// <summary>
/// Abstract base for MassTransit saga state machines.
/// All sagas carry canonical identity, correlation, and lifecycle fields.
/// </summary>
public abstract class SagaStateBase : SagaStateMachineInstance
{
    /// <inheritdoc/>
    public Guid CorrelationId { get; set; }

    /// <summary>Current state name in the saga state machine.</summary>
    public string CurrentState { get; set; } = string.Empty;

    /// <summary>Tenant identity for multi-tenant isolation.</summary>
    public Guid TenantId { get; set; }

    /// <summary>UTC instant at which the saga was created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>UTC instant of the most recent state transition.</summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
