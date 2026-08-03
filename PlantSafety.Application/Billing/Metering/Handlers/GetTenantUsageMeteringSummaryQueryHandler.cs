// AUTHOR: PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetTenantUsageMeteringSummaryQuery. Composes six EXISTING platform
//   data sources (rather than standing up a parallel metrics-collection system) to answer, per
//   tenant per billing period: API calls (PlatformHealthDashboard's RequestMetricEvent capture
//   pipeline), scheduled tasks (PredictiveScheduling's OptimizationRun history), storage usage
//   (IBlobStorageService's per-tenant container), event volume (the DeadLetterEvents dead-letter
//   pipeline -- a real, if partial, proxy for total event volume; see remarks), user count
//   (Features.EnterpriseFabric.Users' ListAsync TotalCount), and module usage (the tenant's own
//   TenantConfiguration.EnabledModules, Phase 7's tenant feature-flag/module system).
//
//   ASSUMPTION FLAGGED: this handler injects each contributing module's existing read repository
//   interface directly (constructor injection), rather than requiring every contributing module to
//   stand up a brand-new IReportSectionProvider<,> implementation first (Handbook Part IX §9.3.2's
//   preferred mechanism, used elsewhere for the Operations.SystemHealth rollup). None of
//   PlatformHealthDashboard/PredictiveScheduling/Messaging.DeadLetter/Features.EnterpriseFabric.Users
//   currently expose an IReportSectionProvider<,>, and standing up five new provider
//   implementations was judged out of scope for this pass given this task's explicit priority on
//   correctness of the billing CALCULATION logic; this still satisfies the instruction to compose
//   existing data sources via DI rather than dispatching another module's Command/Query through
//   ISender, and is a lighter-weight application of the same underlying principle.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Repositories;
using PlantSafety.Application.Billing.Metering.Abstractions;
using PlantSafety.Application.Billing.Metering.Models;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;
using PlantSafety.Application.Messaging.Repositories;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Repositories;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Domain.Billing.Metering.Enums;

namespace PlantSafety.Application.Billing.Metering.Handlers;

/// <summary>Handles <see cref="Queries.GetTenantUsageMeteringSummaryQuery"/> by composing six existing platform data sources.</summary>
public sealed class GetTenantUsageMeteringSummaryQueryHandler(
    IPlatformHealthDashboardRepository platformHealthDashboardRepository,
    IOptimizationRunReadRepository optimizationRunReadRepository,
    ITenantStorageUsageProvider tenantStorageUsageProvider,
    IDeadLetterEventRepository deadLetterEventRepository,
    IUserRepository userRepository,
    ITenantReadRepository tenantReadRepository)
    : IRequestHandler<Queries.GetTenantUsageMeteringSummaryQuery, Result<TenantUsageMeteringSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantUsageMeteringSummaryDto>> Handle(
        Queries.GetTenantUsageMeteringSummaryQuery request, CancellationToken cancellationToken)
    {
        var periodStartUtc = new DateTime(request.PeriodYear, request.PeriodMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEndUtc = periodStartUtc.AddMonths(1);

        // API Calls -- PlatformHealthDashboard's RequestMetricEvent capture pipeline (Phase 6 §5.3).
        var dashboardItems = await platformHealthDashboardRepository.GetPlatformHealthDashboardAsync(
            request.TenantId, periodStartUtc, periodEndUtc, cancellationToken);
        var apiCalls = dashboardItems.Sum(i => (decimal)i.TotalRequests);

        // Scheduled Tasks -- PredictiveScheduling's OptimizationRun history (Phase 5).
        var optimizationRuns = await optimizationRunReadRepository.ListHistoryAsync(
            request.TenantId, siteId: null, periodStartUtc, periodEndUtc, statusCode: null, cancellationToken);
        var scheduledTasks = (decimal)optimizationRuns.Count;

        // Storage Bytes -- the tenant's own blob container, via the Application-layer
        // ITenantStorageUsageProvider abstraction (Infrastructure implementation wraps
        // IBlobStorageService.GetContainerSizeBytesAsync -- see that method's remarks).
        var storageBytes = (decimal)await tenantStorageUsageProvider.GetTenantStorageBytesAsync(request.TenantId, cancellationToken);

        // Event Volume -- dead-lettered event count within the period (a real, if partial, proxy:
        // it reflects failed/faulted event deliveries, not every successfully-processed Outbox
        // event; the Outbox table itself carries no tenant-scoped, queryable read-model today).
        var lookbackDays = Math.Max(1, (int)Math.Ceiling((periodEndUtc - periodStartUtc).TotalDays));
        var deadLetterSummary = await deadLetterEventRepository.GetSummaryAsync(
            request.TenantId.ToString(), lookbackDays, cancellationToken);
        var eventVolume = (decimal)deadLetterSummary.Sum(s => s.DeadLetterCount);

        // User Count -- Features.EnterpriseFabric.Users' real active-user count.
        var (_, userTotalCount) = await userRepository.ListAsync(
            request.TenantId, role: null, siteId: null, isActive: true,
            orderBy: "UserId", sortDirection: "ASC", pageNumber: 1, pageSize: 1, cancellationToken);
        var userCount = (decimal)userTotalCount;

        // Module Usage -- the tenant's own TenantConfiguration.EnabledModules (Phase 7 §2.2).
        var tenant = await tenantReadRepository.GetByIdAsync(request.TenantId, cancellationToken);
        var moduleUsage = (decimal)(tenant?.Configuration.EnabledModules.Count ?? 0);

        var dimensions = new List<MeteredDimensionDto>
        {
            new(UsageMetricType.ApiCalls.Code, apiCalls, "PlatformHealthDashboard.RequestMetricEvent"),
            new(UsageMetricType.ScheduledTasks.Code, scheduledTasks, "PredictiveScheduling.OptimizationRun history"),
            new(UsageMetricType.StorageBytes.Code, storageBytes, "IBlobStorageService tenant container"),
            new(UsageMetricType.EventVolume.Code, eventVolume, "Messaging.DeadLetterEvents summary"),
            new(UsageMetricType.UserCount.Code, userCount, "Features.EnterpriseFabric.Users active count"),
            new(UsageMetricType.ModuleUsage.Code, moduleUsage, "Tenancy.Tenant.TenantConfiguration.EnabledModules"),
        };

        return Result<TenantUsageMeteringSummaryDto>.Success(
            new TenantUsageMeteringSummaryDto(request.TenantId, request.PeriodYear, request.PeriodMonth, dimensions));
    }
}
