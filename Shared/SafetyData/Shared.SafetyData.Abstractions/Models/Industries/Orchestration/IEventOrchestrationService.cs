// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           IEventOrchestrationService.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Runtime entry point interpreting a single real event from
//                  PlantSafety's event domains against the resolved facility
//                  for its tenant. This is the "existing domains publish;
//                  this layer interprets" boundary: it never publishes to
//                  Shared.EventBus, never persists anything, and never
//                  decides what the calling domain should do next -- it only
//                  answers "what does this event mean for this facility's
//                  compliance/risk posture right now". Handle returns null
//                  when the tenant has no registered facility binding
//                  (ITenantFacilityRegistry.TryResolve fails) -- there is
//                  nothing to compute without a resolved facility, and this
//                  is expected to happen for every tenant until it is
//                  explicitly registered (see TenantFacilityRegistration.cs).
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>Interprets a single real runtime event against the resolved facility for its tenant.</summary>
public interface IEventOrchestrationService
{
    /// <summary>
    /// Resolves the facility for <paramref name="tenantId"/> and recomputes its compliance/risk
    /// posture, or returns <see langword="null"/> if no facility is registered for that tenant.
    /// </summary>
    FacilityEventOrchestrationResult? Handle(string tenantId, string sourceModule, string eventType, DateTime occurredAtUtc);
}
