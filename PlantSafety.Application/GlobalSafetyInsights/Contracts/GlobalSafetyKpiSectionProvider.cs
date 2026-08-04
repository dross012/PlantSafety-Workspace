// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GlobalSafetyKpiSectionProvider.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 9 §5.6 Compliance Report Generation)
//  SchemaVersion:  2026.1
//  Description:    GlobalSafetyInsights' implementation of the SharedKernel
//                  IReportSectionProvider&lt;GlobalSafetyKpiSectionRequest, GlobalSafetyInsightsItemDto?&gt;
//                  contract. Dispatches GetGlobalSafetyInsightsQuery via ISender -- fully permitted
//                  because the dispatch stays entirely inside this module -- and adapts the
//                  Application-layer Result&lt;T&gt;/ApplicationError into the SharedKernel's canonical
//                  Result&lt;T&gt;/Error so that Auditing.ComplianceMonitoring's compliance report can
//                  compose the TRIR/LTIR KPI section without referencing
//                  GlobalSafetyInsights.Queries at all. Registered by
//                  GlobalSafetyInsightsApplicationServiceRegistration. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<PlantSafety.Application.GlobalSafetyInsights.Models.GlobalSafetyInsightsItemDto?>;

namespace PlantSafety.Application.GlobalSafetyInsights.Contracts;

/// <summary>
/// GlobalSafetyInsights' <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing the most recently computed TRIR/LTIR KPI snapshot as a public cross-module read-model
/// contract. The section value is <see langword="null"/> (a successful, empty section) when no
/// snapshot has ever been computed for the tenant -- distinct from a failed section.
/// </summary>
public sealed class GlobalSafetyKpiSectionProvider : IReportSectionProvider<GlobalSafetyKpiSectionRequest, GlobalSafetyInsightsItemDto?>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public GlobalSafetyKpiSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "GlobalSafetyKpi";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(GlobalSafetyKpiSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetGlobalSafetyInsightsQuery(request.TenantId, request.From, request.To), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value.Latest)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}
