// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentArchivedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. Enriched V2 counterpart to
//                  AccidentArchivedIntegrationEvent (V1). See
//                  AccidentCreatedIntegrationEventV2.cs's header for the
//                  full design rationale (applies identically here).
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Enriched V2: raised when a Closed Accident is archived for long-term retention.</summary>
public sealed record AccidentArchivedIntegrationEventV2(
    Guid AccidentId,
    string TenantId,
    DateTime ArchivedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId,
    Guid? FacilityId,
    string? IndustryName,
    string? Location,
    string? HazardType,
    DateTime TimestampUtc
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Accidents.AccidentArchivedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
