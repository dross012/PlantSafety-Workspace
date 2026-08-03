// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Typed API client for the Admin Portal's Billing tab. Wraps all HTTP
//   communication for PlantSafety.Api.Billing.Controllers.V1.BillingV1Controller (subscription
//   plans, usage metering, invoices).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>Typed API client for the Admin Portal's Billing tab.</summary>
public sealed class BillingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="BillingApiClient"/>.</summary>
    public BillingApiClient(HttpClient http, ILogger<BillingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the SubscriptionPlan catalog entry currently active for a tenant.</summary>
    public Task<ApiResponseEnvelope<SubscriptionPlanDto>> GetSubscriptionPlanAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
        => GetAsync<SubscriptionPlanDto>($"v1/billing/tenants/{tenantId}/subscription-plan", cancellationToken);

    /// <summary>Composes a tenant's usage across every metered dimension for the given calendar-month billing period (UTC).</summary>
    public Task<ApiResponseEnvelope<TenantUsageMeteringSummaryDto>> GetUsageMeteringAsync(
        Guid tenantId, int periodYear, int periodMonth, CancellationToken cancellationToken = default)
        => GetAsync<TenantUsageMeteringSummaryDto>(
            $"v1/billing/tenants/{tenantId}/usage-metering?periodYear={periodYear}&periodMonth={periodMonth}", cancellationToken);

    /// <summary>Lists every invoice generated for a tenant, most recent period first.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<InvoiceDto>>> ListInvoicesAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<InvoiceDto>>($"v1/billing/tenants/{tenantId}/invoices", cancellationToken);

    /// <summary>Retrieves a single invoice by identity.</summary>
    public Task<ApiResponseEnvelope<InvoiceDto>> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        => GetAsync<InvoiceDto>($"v1/billing/invoices/{invoiceId}", cancellationToken);

    /// <summary>Generates a Draft invoice for the given tenant/billing period.</summary>
    public Task<ApiResponseEnvelope<InvoiceDto>> GenerateInvoiceAsync(
        Guid tenantId, GenerateInvoiceRequest request, CancellationToken cancellationToken = default)
        => PostAsync<GenerateInvoiceRequest, InvoiceDto>($"v1/billing/tenants/{tenantId}/invoices", request, cancellationToken);

    /// <summary>Issues (if Draft) and attempts to charge the given invoice via the configured payment processor.</summary>
    public Task<ApiResponseEnvelope<InvoiceDto>> ChargeInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        => PostAsync<object, InvoiceDto>($"v1/billing/invoices/{invoiceId}/charge", new { }, cancellationToken);

    /// <summary>Refunds a previously paid invoice's charge and voids the invoice.</summary>
    public Task<ApiResponseEnvelope<bool>> RefundInvoiceAsync(
        Guid invoiceId, RefundInvoiceRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RefundInvoiceRequest, bool>($"v1/billing/invoices/{invoiceId}/refund", request, cancellationToken);
}
