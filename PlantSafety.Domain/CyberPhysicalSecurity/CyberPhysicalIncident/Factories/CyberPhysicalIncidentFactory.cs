// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default implementation of ICyberPhysicalIncidentFactory. Derives the incident
//                  title/severity/related-indicator list from a set of escalated SabotageIndicators,
//                  implementing the "correlated multi-source indicators auto-escalate" risk hook's
//                  downstream Incident Response step.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Factories;

/// <summary>Default <see cref="ICyberPhysicalIncidentFactory"/> implementation.</summary>
public sealed class CyberPhysicalIncidentFactory : ICyberPhysicalIncidentFactory
{
    /// <inheritdoc/>
    public Aggregates.CyberPhysicalIncident CreateFromEscalatedIndicators(
        Guid tenantId,
        Guid siteId,
        UserId ownerId,
        UserId createdBy,
        IReadOnlyCollection<SabotageIndicatorAggregate> escalatedIndicators)
    {
        if (escalatedIndicators.Count == 0)
            throw new ArgumentException("At least one escalated indicator is required to open a CyberPhysicalIncident.", nameof(escalatedIndicators));

        var severity = escalatedIndicators.Any(i => i.Severity == SabotageIndicatorSeverity.Critical)
            ? CyberPhysicalIncidentSeverity.Critical
            : CyberPhysicalIncidentSeverity.High;

        var sourceSummary = string.Join(", ", escalatedIndicators.Select(i => i.Source).Distinct());
        var title = $"Correlated cyber-physical sabotage indicators ({sourceSummary})";
        var description = string.Join(Environment.NewLine, escalatedIndicators.Select(i => $"- [{i.Source}] {i.Description}"));

        var now = UtcTimestamp.Now();

        return Aggregates.CyberPhysicalIncident.Open(
            createdBy: createdBy,
            createdAtUtc: now,
            correlationId: CorrelationId.New(),
            causationId: CausationId.New(),
            actor: PerformedByActor.System(),
            tenantId: tenantId,
            siteId: siteId,
            title: title,
            description: description,
            severity: severity,
            ownerId: ownerId,
            relatedSabotageIndicatorIds: escalatedIndicators.Select(i => i.SabotageIndicatorId.Value));
    }
}
