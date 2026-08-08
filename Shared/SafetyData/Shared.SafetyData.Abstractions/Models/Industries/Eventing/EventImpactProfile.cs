// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Eventing
//  Layer:          Abstractions
//  File:           EventImpactProfile.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Facility-specific computed impact of a single event type:
//                  given an EventDefinition and a FacilityProfile, what does
//                  this event type actually mean for THIS facility --
//                  cross-referencing the event's generic RequiredSops/
//                  RequiredTraining/RequiredPermits against the facility's
//                  own FacilityComplianceProfile.MissingElements, so
//                  RequiredCorrectiveActions reflects genuine gaps at this
//                  specific facility rather than the event type's generic
//                  requirement list. This is the read-only "what does it
//                  imply" computation a real event domain (AccidentManagement,
//                  SafetyEvents, etc.) is expected to call into -- see
//                  EventDefinition.cs's ARCHITECTURAL SCOPE note. Additive-only
//                  evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.Eventing;

/// <summary>
/// The facility-specific computed impact of a single event type occurring at a specific
/// facility.
/// </summary>
/// <param name="EventCode">The event type this impact was computed for.</param>
/// <param name="FacilityId">The facility this impact was computed for.</param>
/// <param name="RequiredActions">SOPs to follow in response (the event's <see cref="EventDefinition.RequiredSops"/>).</param>
/// <param name="RequiredNotifications">Who/what must be notified: one entry per <see cref="EventDefinition.RelatedRegulators"/>, plus the facility safety officer.</param>
/// <param name="RequiredDocumentation">Documentation obligations: an incident report plus one entry per <see cref="EventDefinition.RequiredPermits"/>.</param>
/// <param name="RequiredFollowUpAudits">Audit checklist items to perform as follow-up (one per <see cref="EventDefinition.RelatedPrograms"/>).</param>
/// <param name="RequiredCorrectiveActions">
/// The subset of the facility's own <see cref="FacilityComplianceProfile.MissingElements"/> whose
/// code matches one of this event's required SOP/training/permit codes -- the genuine,
/// facility-specific compliance gaps this event exposes.
/// </param>
/// <param name="ComplianceImpact">A short, deterministic summary of the corrective-action gap (e.g. "3 of 5 related compliance elements missing").</param>
/// <param name="RiskImpact">A short, deterministic summary combining the event's <see cref="EventDefinition.Severity"/> with the facility's current <see cref="ComplianceRiskLevel"/>.</param>
public sealed record EventImpactProfile(
    string EventCode,
    Guid FacilityId,
    IReadOnlyList<SopTemplate> RequiredActions,
    IReadOnlyList<string> RequiredNotifications,
    IReadOnlyList<string> RequiredDocumentation,
    IReadOnlyList<AuditChecklistItem> RequiredFollowUpAudits,
    IReadOnlyList<ComplianceElement> RequiredCorrectiveActions,
    string ComplianceImpact,
    string RiskImpact);

/// <summary>Builds <see cref="EventImpactProfile"/> instances deterministically from an <see cref="EventDefinition"/> and a <see cref="FacilityProfile"/>.</summary>
public static class EventImpactProfileBuilder
{
    /// <summary>Builds the facility-specific impact of the given event type occurring at the given facility.</summary>
    public static EventImpactProfile Build(EventDefinition eventDefinition, FacilityProfile facility)
    {
        IReadOnlyList<string> requiredNotifications = eventDefinition.RelatedRegulators
            .Select(regulator => $"Notify {regulator.Code()} per applicable reporting obligations")
            .Append("Notify facility safety officer")
            .ToArray();

        IReadOnlyList<string> requiredDocumentation = new[] { "File incident report" }
            .Concat(eventDefinition.RequiredPermits.Select(permit => $"Document {permit} compliance status"))
            .ToArray();

        IReadOnlyList<AuditChecklistItem> requiredFollowUpAudits = eventDefinition.RelatedPrograms
            .Select(ProgramAuditRequirements.For)
            .ToArray();

        var eventElementCodes = eventDefinition.RequiredSops.Select(sop => sop.Code)
            .Concat(eventDefinition.RequiredTraining.Select(training => training.Code))
            .Concat(eventDefinition.RequiredPermits.Select(permit => permit.ToString()))
            .ToHashSet(StringComparer.Ordinal);

        IReadOnlyList<ComplianceElement> requiredCorrectiveActions = facility.ComplianceProfile.MissingElements
            .Where(element => eventElementCodes.Contains(element.Code))
            .ToArray();

        string complianceImpact = eventElementCodes.Count == 0
            ? "No facility-specific compliance elements tied to this event type."
            : $"{requiredCorrectiveActions.Count} of {eventElementCodes.Count} related compliance elements missing at this facility.";

        string riskImpact = $"Event severity {eventDefinition.Severity} against facility compliance risk {facility.ComplianceProfile.RiskLevel}.";

        return new EventImpactProfile(
            EventCode: eventDefinition.EventCode,
            FacilityId: facility.FacilityId,
            RequiredActions: eventDefinition.RequiredSops,
            RequiredNotifications: requiredNotifications,
            RequiredDocumentation: requiredDocumentation,
            RequiredFollowUpAudits: requiredFollowUpAudits,
            RequiredCorrectiveActions: requiredCorrectiveActions,
            ComplianceImpact: complianceImpact,
            RiskImpact: riskImpact);
    }
}
