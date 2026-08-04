// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GlobalSafetyInsightsApplicationServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2/§4 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the GlobalSafetyInsights Application-layer
//                  domain-service singletons (pure, stateless, side-effect-free -- same convention
//                  as the Phase 7 MultiRegion/DataResidency registrations). MediatR handler
//                  discovery is assembly-wide (ApplicationServiceRegistration.AddApplicationServices),
//                  so no explicit handler registration is required here. MUST be invoked from
//                  ApplicationServiceRegistration.AddApplicationServices -- see that file's call
//                  site. Additive-only evolution.
//                  2026-08-03 (Phase 9 §5.6 Compliance Report Generation): also registers this
//                  module's two SharedKernel IReportSectionProvider&lt;,&gt; implementations
//                  (GlobalSafetyKpiSectionProvider, CrossSiteActionLogSectionProvider), letting
//                  Auditing.ComplianceMonitoring's compliance report compose the TRIR/LTIR KPI and
//                  cross-site-action sections via DI instead of dispatching this module's Queries
//                  directly through ISender (Handbook Part IX §9.3.2).
// ============================================================================
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.Application.GlobalSafetyInsights.Contracts;
using PlantSafety.Application.GlobalSafetyInsights.Coordination;
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.Services;

namespace PlantSafety.Application.GlobalSafetyInsights.DependencyInjection;

/// <summary>Registers Application-layer services for the GlobalSafetyInsights module.</summary>
public static class GlobalSafetyInsightsApplicationServiceRegistration
{
    /// <summary>Adds GlobalSafetyInsights domain-service singletons to the service collection.</summary>
    public static IServiceCollection AddGlobalSafetyInsightsApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalSafetyTelemetryNormalizer, GlobalSafetyTelemetryNormalizer>();
        services.AddSingleton<IGlobalSafetyKpiEngine, GlobalSafetyKpiEngine>();
        services.AddSingleton<IGlobalSafetyAnomalyDetector, GlobalSafetyAnomalyDetector>();
        services.AddSingleton<IGlobalSafetyRiskScorer, GlobalSafetyRiskScorer>();
        services.AddSingleton<GlobalSafetyRecommendationComposer>();
        services.AddSingleton<IGlobalSafetyInsightsFactory, Domain.GlobalSafetyInsights.Factories.GlobalSafetyInsightsFactory>();

        // Phase 8 §3 Multi-Site Coordination Workflows: deterministic domain policy + real
        // orchestration router + in-process anomaly-to-escalation reactor.
        services.AddSingleton<IMultiSiteSafetyPolicy, MultiSiteSafetyPolicy>();
        services.AddScoped<IMultiSiteActionRouter, MultiSiteActionRouter>();
        services.AddScoped<IGlobalSafetyAnomalyEscalationReactor, GlobalSafetyAnomalyEscalationReactor>();

        // Phase 9 §5.6 Compliance Report Generation: cross-module read-model composition providers.
        services.AddScoped<IReportSectionProvider<GlobalSafetyKpiSectionRequest, GlobalSafetyInsightsItemDto?>, GlobalSafetyKpiSectionProvider>();
        services.AddScoped<IReportSectionProvider<CrossSiteActionLogSectionRequest, IReadOnlyList<CrossSiteActionLogEntryDto>>, CrossSiteActionLogSectionProvider>();

        return services;
    }
}
