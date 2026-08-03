// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.EventMesh
//  Layer:          Application
//  File:           EnterpriseEventMeshApplicationServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3 Enterprise Event Mesh, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Registers the Enterprise Event Mesh's Application-layer services: every
//                  IEnterpriseInboundEventTranslator implementation (resolved via DI by
//                  IngestEnterpriseInboundEventCommandHandler, per the
//                  IReportSectionProvider&lt;,&gt; precedent -- never dispatched as a transferable
//                  cross-module object) plus the EventUpgradePipeline singleton. MediatR command
//                  handlers are auto-registered via the existing Application assembly scan. Wired
//                  into the composition root (ApplicationServiceRegistration.AddApplicationServices).
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.Enterprise.EventMesh.Translators;
using PlantSafety.Application.Enterprise.Integrations.Cmms.Translators;
using PlantSafety.Application.Enterprise.Integrations.Erp.Translators;
using PlantSafety.Application.Enterprise.Integrations.Hr.Translators;
using PlantSafety.Application.Enterprise.Integrations.Mes.Translators;
using PlantSafety.Application.Enterprise.Integrations.Vendor.Translators;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Application.Enterprise.EventMesh.DependencyInjection;

/// <summary>Registers the Enterprise Event Mesh's Application-layer services.</summary>
public static class EnterpriseEventMeshApplicationServiceRegistration
{
    /// <summary>Adds every inbound translator and the event-upgrade pipeline to the service collection.</summary>
    public static IServiceCollection AddEnterpriseEventMeshApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEnterpriseInboundEventTranslator, ErpWorkOrderStatusSyncInboundTranslator>();

        // Phase 6 §1 Enterprise System Integrations wave (2026-08-03): ERP/MES/CMMS/HR/Vendor
        // inbound translators. See each translator's own header for the real internal command/
        // ingestion surface it dispatches to.
        services.AddScoped<IEnterpriseInboundEventTranslator, ErpAssetMasterUpdateInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, MesProductionOrderUpdateInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, MesTelemetryInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, MesQualityCheckInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, CmmsWorkOrderStatusSyncInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, CmmsTechnicianAssignmentInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, CmmsMaintenancePlanScheduleInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, CmmsMaintenanceCompletedInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, HrUserProvisioningInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, HrTrainingCompletionInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, VendorTelemetryInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, VendorPredictiveMaintenanceInboundTranslator>();
        services.AddScoped<IEnterpriseInboundEventTranslator, VendorRiskScoreInboundTranslator>();

        services.AddSingleton<EventUpgradePipeline>();

        return services;
    }
}
