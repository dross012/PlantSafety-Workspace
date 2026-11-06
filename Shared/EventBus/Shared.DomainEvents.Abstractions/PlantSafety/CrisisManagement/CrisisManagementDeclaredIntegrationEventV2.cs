// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.CrisisManagement
//  File:           CrisisManagementDeclaredIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. First-ever cross-module integration contract
//                  for CrisisManagement -- CrisisManagement has never had
//                  any Shared.EventBus coverage before this file (verified:
//                  no ICrossModuleEventRelay usage anywhere in
//                  PlantSafety.Infrastructure.SafetyEvents.CrisisManagement
//                  prior to this phase). Mirrors PlantSafety.Domain.
//                  SafetyEvents.CrisisManagement.Events.
//                  CrisisManagementDeclaredEvent (the real domain event,
//                  unmodified) -- this is what the phase's own spec named
//                  "CrisisEventDeclaredEventV2." See
//                  AccidentCreatedIntegrationEventV2.cs's header for the
//                  general V2 design rationale.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.CrisisManagement;

/// <summary>Enriched V2: raised when a new crisis is declared.</summary>
public sealed record CrisisManagementDeclaredIntegrationEventV2(
    Guid CrisisManagementId,
    string TenantId,
    Guid? SiteId,
    string Title,
    string Severity,
    DateTime DeclaredAtUtc,
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
    public static string EventType => "CrisisManagement.CrisisManagementDeclaredV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
