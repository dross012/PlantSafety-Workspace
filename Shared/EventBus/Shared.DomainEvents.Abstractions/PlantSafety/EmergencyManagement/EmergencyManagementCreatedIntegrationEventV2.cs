// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.EmergencyManagement
//  File:           EmergencyManagementCreatedIntegrationEventV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3. First-ever cross-module integration contract
//                  for EmergencyManagement (same situation as
//                  CrisisManagementDeclaredIntegrationEventV2.cs -- verified
//                  no prior ICrossModuleEventRelay usage anywhere in
//                  PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement).
//                  Mirrors PlantSafety.Domain.SafetyEvents.EmergencyManagement.
//                  Events.EmergencyManagementCreatedEvent (the real domain
//                  event, unmodified) -- this is what the phase's own spec
//                  named "EmergencyEventTriggeredEventV2." See
//                  AccidentCreatedIntegrationEventV2.cs's header for the
//                  general V2 design rationale.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.EmergencyManagement;

/// <summary>Enriched V2: raised when a new EmergencyManagement aggregate is created in PlantSafety.</summary>
public sealed record EmergencyManagementCreatedIntegrationEventV2(
    Guid EmergencyManagementId,
    string TenantId,
    DateTime CreatedAtUtc,
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
    public static string EventType => "EmergencyManagement.EmergencyManagementCreatedV2";
    /// <inheritdoc/>
    public static int SchemaVersion => 2;
}
