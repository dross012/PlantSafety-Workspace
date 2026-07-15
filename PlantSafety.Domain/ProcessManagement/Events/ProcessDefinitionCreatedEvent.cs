// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionCreatedEvent.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a new ProcessDefinition aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Events;

/// <summary>Raised when a new <c>ProcessDefinition</c> aggregate is created.</summary>
public sealed record ProcessDefinitionCreatedEvent(
    Guid              EventId,
    Guid              ProcessDefinitionId,
    Guid              TenantId,
    string            ProcessName,
    string            Category,
    UtcTimestamp      CreatedAtUtc,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : IProcessDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProcessDefinitionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
