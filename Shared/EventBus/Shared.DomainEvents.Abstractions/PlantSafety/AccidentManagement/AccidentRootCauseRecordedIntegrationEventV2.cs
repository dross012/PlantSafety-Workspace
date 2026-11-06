// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.AccidentManagement
//  File:           AccidentRootCauseRecordedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. Enriched V2 DTO for PlantSafety.Domain.
//                  AccidentManagement.Events.AccidentRootCauseRecordedEvent
//                  -- previously uncovered by any cross-module contract
//                  (see AccidentInvestigationClosedIntegrationEventV2.cs's
//                  header). RootCause is the real RootCauseCategory enum's
//                  ToString(), matching this codebase's established
//                  convention for mapping domain enums onto integration DTOs
//                  (see CrossModuleDomainEventMapper.cs's Compliance case
//                  arms).
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;

/// <summary>Enriched V2: raised when Root Cause Analysis completes and a root cause is recorded.</summary>
public sealed record AccidentRootCauseRecordedIntegrationEventV2(
    Guid AccidentId,
    string TenantId,
    string RootCause,
    DateTime RecordedAtUtc,
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
    public static string EventType => "Accidents.AccidentRootCauseRecordedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
