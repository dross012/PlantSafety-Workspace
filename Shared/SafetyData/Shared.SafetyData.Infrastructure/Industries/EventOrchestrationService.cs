// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           EventOrchestrationService.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    IEventOrchestrationService implementation.
//
//  INVESTIGATION TRAIL (why this doesn't call EventRoutingEngine/EventCatalog):
//  Before building this, the real PlantSafety domain events available today
//  were read directly: PlantSafety.Domain.AccidentManagement.Events.*,
//  PlantSafety.Domain.SafetyEvents.{CrisisManagement,EmergencyManagement}.Events.*,
//  and their Shared.EventBus integration-event projections under
//  Shared.DomainEvents.Abstractions.PlantSafety.*. Findings:
//    1. None of them are hazard-typed the way Eventing.EventCatalog's 21
//       entries are (chemical spill, fire, injury, etc.) -- they are generic
//       aggregate-lifecycle events (Created/InvestigatorAssigned/
//       RootCauseRecorded/CorrectiveActionCreated/RegulatoryFilingCompleted/
//       InvestigationClosed/Archived). There is no field anywhere indicating
//       *what kind* of accident/emergency occurred, so there is no real data
//       to select an EventCatalog entry from -- doing so would mean
//       guessing/inventing a mapping, which this subsystem's own conventions
//       (see EventCatalog.cs) explicitly avoid.
//    2. None of them carry facilityId/industryName/location. The only
//       facility-adjacent field on record is TenantId (a raw string on
//       EventEnvelope and every integration event DTO) -- hence
//       ITenantFacilityRegistry existing as the interim correlation
//       mechanism.
//    3. Only AccidentManagement and RiskManagement (of the domains named in
//       this pass's brief) currently have integration-event projections
//       published to Shared.EventBus at all. SafetyEvents.CrisisManagement
//       and SafetyEvents.EmergencyManagement have zero -- there is nothing
//       to subscribe to for those two yet; adding that is a separate,
//       larger task (new integration-event DTOs plus wiring a publisher
//       into those domains' Application/Infrastructure layers) which was
//       not undertaken here.
//  Given that, this service computes the one thing that IS fully grounded in
//  real data end to end: "a real event happened for a real (resolved)
//  tenant -> recompute that facility's actual compliance/risk posture."
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>Stateless <see cref="IEventOrchestrationService"/> implementation.</summary>
public sealed class EventOrchestrationService(
    ITenantFacilityRegistry tenantFacilityRegistry,
    IFacilityProfileService facilityProfileService,
    IComplianceRiskService complianceRiskService) : IEventOrchestrationService
{
    /// <inheritdoc />
    public FacilityEventOrchestrationResult? Handle(string tenantId, string sourceModule, string eventType, DateTime occurredAtUtc)
    {
        if (!tenantFacilityRegistry.TryResolve(tenantId, out TenantFacilityRegistration? registration))
        {
            return null;
        }

        FacilityProfile facility = facilityProfileService.BuildProfile(
            registration!.FacilityId, registration.Name, registration.IndustryName, registration.Location);

        ComplianceScore complianceScore = complianceRiskService.ComputeComplianceScore(facility);
        RiskScore riskScore = complianceRiskService.ComputeRiskScore(facility);
        ComplianceDashboardMetadata complianceDashboard = complianceRiskService.ComputeComplianceDashboard(facility);
        RiskDashboardMetadata riskDashboard = complianceRiskService.ComputeRiskDashboard(facility);
        (ComplianceReadiness complianceReadiness, RiskReadiness riskReadiness) = complianceRiskService.ComputeReadiness(facility);

        IReadOnlyList<string> dashboardAlerts = complianceDashboard.RequiredAlerts
            .Concat(riskDashboard.RequiredAlerts)
            .ToArray();

        string notification =
            $"{sourceModule} raised '{eventType}' for facility '{registration.Name}' ({registration.IndustryName}) at {occurredAtUtc:O}. " +
            $"Compliance {complianceScore.PercentComplete:F1}% ({complianceScore.ComplianceTier}); risk {riskScore.OverallRiskLevel}.";

        OrchestrationEventCategory category = OrchestrationEventCategoryCatalog.ForEventType(eventType);

        bool requiresImmediateAttention =
            riskScore.OverallRiskLevel is RiskLevel.High or RiskLevel.Critical
            || complianceScore.ComplianceTier == ComplianceTier.NonCompliant;

        return new FacilityEventOrchestrationResult(
            TenantId: tenantId,
            FacilityId: registration.FacilityId,
            SourceModule: sourceModule,
            EventType: eventType,
            OccurredAtUtc: occurredAtUtc,
            ComplianceScore: complianceScore,
            RiskScore: riskScore,
            ComplianceReadiness: complianceReadiness,
            RiskReadiness: riskReadiness,
            DashboardAlerts: dashboardAlerts,
            Notification: notification,
            Category: category,
            RequiresImmediateAttention: requiresImmediateAttention);
    }
}
