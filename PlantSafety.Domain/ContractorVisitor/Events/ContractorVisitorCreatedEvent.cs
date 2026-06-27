// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitorCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a ContractorVisitor aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Events;

/// <summary>Raised when a new <see cref="ContractorVisitor"/> aggregate is created.</summary>
public sealed record ContractorVisitorCreatedEvent(
    Guid          EventId,
    Guid          ContractorVisitorId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IContractorVisitorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorVisitorCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
