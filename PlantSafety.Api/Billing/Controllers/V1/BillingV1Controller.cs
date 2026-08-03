// ============================================================================
// File: BillingV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.1 Admin Portal / Billing)
// Created: 2026-08-03
// Description: V1 controller exposing the Billing bounded context (subscription plans,
//              usage metering, invoices) to the Admin Portal UI. Built as a thin HTTP
//              boundary over the already-existing Billing Application layer
//              (PlantSafety.Application.Billing.*) -- no business logic here. This
//              controller was genuinely missing prior to Phase 7 §6.1 (the Application
//              layer's Billing CQRS existed with no corresponding Api controller); it is
//              added here rather than inventing fictional routes in the UI layer.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//   GET  api/v1/billing/tenants/{tenantId}/subscription-plan             -- active plan for tenant
//   GET  api/v1/billing/tenants/{tenantId}/usage-metering                -- usage summary for a period
//   GET  api/v1/billing/tenants/{tenantId}/invoices                      -- list invoices for a tenant
//   GET  api/v1/billing/invoices/{invoiceId}                             -- get a single invoice
//   POST api/v1/billing/tenants/{tenantId}/invoices                      -- generate a draft invoice
//   POST api/v1/billing/invoices/{invoiceId}/charge                      -- issue + charge an invoice
//   POST api/v1/billing/invoices/{invoiceId}/refund                      -- refund a paid invoice
//
// ============================================================================

using PlantSafety.Api.Billing.Authorization;
using PlantSafety.Api.Billing.Contracts.Requests;
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Application.Billing.BillingEngine.Commands;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Billing.BillingEngine.Queries;
using PlantSafety.Application.Billing.Metering.Models;
using PlantSafety.Application.Billing.Metering.Queries;
using PlantSafety.Application.Billing.Subscription.Models;
using PlantSafety.Application.Billing.Subscription.Queries;

namespace PlantSafety.Api.Billing.Controllers.V1;

/// <summary>V1 controller for the Billing bounded context (subscription plans, usage metering, invoices).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billing")]
[Authorize(Policy = BillingAuthorizationPolicies.Manage)]
[Tags("Billing")]
public sealed class BillingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="BillingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public BillingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves the SubscriptionPlan catalog entry currently active for a tenant.</summary>
    /// <param name="tenantId">Tenant identifier to resolve the active plan for.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("tenants/{tenantId:guid}/subscription-plan")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptionPlanAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetSubscriptionPlanForTenantQuery(tenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Composes a tenant's usage across every metered dimension for the given calendar-month billing period (UTC).</summary>
    /// <param name="tenantId">Tenant to compose usage metering for.</param>
    /// <param name="periodYear">Calendar year of the billing period (UTC).</param>
    /// <param name="periodMonth">Calendar month of the billing period (UTC), 1-12.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("tenants/{tenantId:guid}/usage-metering")]
    [ProducesResponseType(typeof(ApiResponse<TenantUsageMeteringSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantUsageMeteringSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsageMeteringAsync(
        Guid tenantId,
        [FromQuery] int periodYear,
        [FromQuery] int periodMonth,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetTenantUsageMeteringSummaryQuery(tenantId, periodYear, periodMonth);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists every invoice generated for a tenant, most recent period first.</summary>
    /// <param name="tenantId">Tenant identity.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("tenants/{tenantId:guid}/invoices")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvoiceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvoiceDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListInvoicesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetInvoiceListQuery(tenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single invoice by identity.</summary>
    /// <param name="invoiceId">Invoice identity.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("invoices/{invoiceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetInvoiceQuery(invoiceId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Generates a Draft invoice for the given tenant/billing period from an already-captured usage-metering snapshot.</summary>
    /// <param name="tenantId">Tenant to generate the invoice for.</param>
    /// <param name="request">Billing period and discounts/credits.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("tenants/{tenantId:guid}/invoices")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInvoiceAsync(
        Guid tenantId,
        [FromBody] GenerateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var discountsAndCredits = request.DiscountsAndCredits
            .Select(d => new DiscountOrCreditRequestDto(d.Kind, d.Description, d.PercentageValue, d.FixedAmount))
            .ToList();

        var command = new GenerateInvoiceCommand(tenantId, request.PeriodYear, request.PeriodMonth, discountsAndCredits);
        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Issues (if Draft) and attempts to charge the given invoice via the configured payment processor.</summary>
    /// <param name="invoiceId">Invoice to charge.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("invoices/{invoiceId:guid}/charge")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChargeInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var result = await _sender.Send(new ChargeInvoiceCommand(invoiceId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Refunds a previously paid invoice's charge and voids the invoice.</summary>
    /// <param name="invoiceId">Invoice to refund.</param>
    /// <param name="request">Refund reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("invoices/{invoiceId:guid}/refund")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefundInvoiceAsync(
        Guid invoiceId,
        [FromBody] RefundInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var result = await _sender.Send(new RefundInvoiceCommand(invoiceId, request.Reason), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
