// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Queries for retrieving Invoice read models.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.BillingEngine.Queries;

/// <summary>Retrieves a single invoice by identity.</summary>
/// <param name="InvoiceId">Invoice identity.</param>
public sealed record GetInvoiceQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;

/// <summary>Lists every invoice generated for a tenant, most recent period first.</summary>
/// <param name="TenantId">Tenant identity.</param>
public sealed record GetInvoiceListQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<InvoiceDto>>>;
