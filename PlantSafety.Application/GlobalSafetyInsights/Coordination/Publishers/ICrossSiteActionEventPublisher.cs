// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           ICrossSiteActionEventPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Application-layer abstraction over publishing a CrossSiteActionEvent onto the
//                  real internal Outbox -> MassTransit pipeline, so that CrossSiteActionConsumer
//                  (already built, Phase 8 §4) receives it and records it into the
//                  GlobalSafetyInsights audit ledger. Implemented in Infrastructure as
//                  CrossSiteActionEventPublisher, mirroring IGlobalSafetyInsightsPublisher's
//                  existing pattern rather than inventing a second publishing mechanism.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Events;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Publishers;

/// <summary>Publishes a <see cref="CrossSiteActionEvent"/> onto the real internal event mesh.</summary>
public interface ICrossSiteActionEventPublisher
{
    /// <summary>Publishes <paramref name="evt"/>, respecting data residency, so CrossSiteActionConsumer can record it into the audit ledger.</summary>
    Task PublishAsync(CrossSiteActionEvent evt, CancellationToken cancellationToken);
}
