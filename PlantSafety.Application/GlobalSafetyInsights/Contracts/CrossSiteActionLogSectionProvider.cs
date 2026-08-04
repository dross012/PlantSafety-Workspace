// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           CrossSiteActionLogSectionProvider.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 9 §5.6 Compliance Report Generation)
//  SchemaVersion:  2026.1
//  Description:    GlobalSafetyInsights' implementation of the SharedKernel
//                  IReportSectionProvider&lt;CrossSiteActionLogSectionRequest,
//                  IReadOnlyList&lt;CrossSiteActionLogEntryDto&gt;&gt; contract. Dispatches
//                  GetCrossSiteActionHistoryQuery via ISender -- fully permitted because the
//                  dispatch stays entirely inside this module -- and adapts the Application-layer
//                  Result&lt;T&gt;/ApplicationError into the SharedKernel's canonical Result&lt;T&gt;/Error
//                  so that Auditing.ComplianceMonitoring's compliance report can compose the
//                  cross-site-actions section without referencing GlobalSafetyInsights.Queries at
//                  all. Registered by GlobalSafetyInsightsApplicationServiceRegistration.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<System.Collections.Generic.IReadOnlyList<PlantSafety.Application.GlobalSafetyInsights.Models.CrossSiteActionLogEntryDto>>;

namespace PlantSafety.Application.GlobalSafetyInsights.Contracts;

/// <summary>
/// GlobalSafetyInsights' <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing the CrossSiteAction audit ledger for a date range as a public cross-module read-model
/// contract.
/// </summary>
public sealed class CrossSiteActionLogSectionProvider : IReportSectionProvider<CrossSiteActionLogSectionRequest, IReadOnlyList<CrossSiteActionLogEntryDto>>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public CrossSiteActionLogSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "CrossSiteActionLog";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(CrossSiteActionLogSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCrossSiteActionHistoryQuery(request.TenantId, request.From, request.To), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}
