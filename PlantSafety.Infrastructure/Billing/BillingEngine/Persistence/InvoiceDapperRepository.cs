// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Infrastructure
//  File:           InvoiceDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IInvoiceRepository. Line items are persisted
//                  as a JSON array (mirroring Tenant's ConfigurationJson / UsageMeteringSnapshot's
//                  RecordedValuesJson convention) rather than a normalized child table, since an
//                  invoice's line items are always read/written as a single immutable unit
//                  alongside their parent invoice. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.Aggregates;
using PlantSafety.Domain.Billing.BillingEngine.Enums;
using PlantSafety.Domain.Billing.BillingEngine.Interfaces;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Infrastructure.Billing.BillingEngine.Persistence;

/// <summary>Dapper-backed repository for <see cref="Invoice"/> persistence.</summary>
public sealed class InvoiceDapperRepository(
    ISqlConnectionFactory connectionFactory, DapperQueryExecutor queryExecutor, DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter, ILogger<InvoiceDapperRepository> logger)
    : IInvoiceRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Invoice {InvoiceId} Tenant {TenantId} Period {Period}", invoice.InvoiceId.Value, invoice.TenantId.Value, invoice.Period);

        var parameters = BuildInsertParameters(invoice);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        await commandExecutor.ExecuteAsync(conn, "[Billing].[usp_Invoice_Insert]", parameters, tx, cancellationToken: cancellationToken);

        tx.Commit();

        FlushDomainEvents(invoice);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Invoice {InvoiceId} Status {Status}", invoice.InvoiceId.Value, invoice.Status.Code);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("InvoiceId", invoice.InvoiceId.Value)
            .AddString("Status", invoice.Status.Code, 20)
            .AddDateTime("IssuedAtUtc", invoice.IssuedAtUtc?.Value)
            .AddDateTime("PaidAtUtc", invoice.PaidAtUtc?.Value)
            .AddString("PaymentProcessorTransactionId", invoice.PaymentProcessorTransactionId, 200)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        await commandExecutor.ExecuteAsync(conn, "[Billing].[usp_Invoice_Update]", parameters, tx, cancellationToken: cancellationToken);

        tx.Commit();

        FlushDomainEvents(invoice);
    }

    /// <inheritdoc/>
    public async Task<Invoice?> GetByIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("InvoiceId", invoiceId.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<InvoiceRow>("[Billing].[usp_Invoice_GetById]", parameters, cancellationToken);
        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<Invoice?> GetByTenantAndPeriodAsync(TenantId tenantId, BillingPeriod period, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId.Value)
            .AddInt("PeriodYear", period.Year)
            .AddInt("PeriodMonth", period.Month)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<InvoiceRow>("[Billing].[usp_Invoice_GetByTenantAndPeriod]", parameters, cancellationToken);
        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Invoice>> ListByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId.Value).Build();
        var rows = await queryExecutor.QueryAsync<InvoiceRow>("[Billing].[usp_Invoice_ListByTenant]", parameters, cancellationToken);
        return rows.Select(Hydrate).ToList();
    }

    private void FlushDomainEvents(Invoice invoice)
    {
        foreach (var domainEvent in invoice.DomainEvents)
            outboxWriter.Enqueue(domainEvent, invoice.InvoiceId.Value.ToString(), "system", Guid.Empty.ToString());
        invoice.ClearDomainEvents();
    }

    private static DynamicParameters BuildInsertParameters(Invoice invoice) => new StoredProcedureBuilder()
        .AddGuid("InvoiceId", invoice.InvoiceId.Value)
        .AddGuid("TenantId", invoice.TenantId.Value)
        .AddInt("PeriodYear", invoice.Period.Year)
        .AddInt("PeriodMonth", invoice.Period.Month)
        .AddString("Currency", invoice.TotalDue.Currency, 3)
        .AddString("LineItemsJson", SerializeLineItems(invoice.LineItems), size: -1)
        .AddDecimal("SubtotalAmount", invoice.Subtotal.Amount)
        .AddDecimal("TotalDueAmount", invoice.TotalDue.Amount)
        .AddString("Status", invoice.Status.Code, 20)
        .AddDateTime("GeneratedAtUtc", invoice.GeneratedAtUtc.Value)
        .AddDateTime("IssuedAtUtc", invoice.IssuedAtUtc?.Value)
        .AddDateTime("PaidAtUtc", invoice.PaidAtUtc?.Value)
        .AddString("PaymentProcessorTransactionId", invoice.PaymentProcessorTransactionId, 200)
        .Build();

    private static string SerializeLineItems(IReadOnlyList<InvoiceLineItem> lineItems) => JsonSerializer.Serialize(
        lineItems.Select(li => new LineItemRow(li.Type.ToString(), li.Description, li.Quantity, li.UnitPrice.Amount, li.Amount.Amount)).ToList());

    private static Invoice Hydrate(InvoiceRow row)
    {
        var lineItemRows = JsonSerializer.Deserialize<List<LineItemRow>>(row.LineItemsJson) ?? [];
        var lineItems = lineItemRows.Select(r => new InvoiceLineItem(
            Enum.Parse<LineItemType>(r.Type), r.Description, r.Quantity,
            new PlantSafety.SharedKernel.Primitives.ValueTypes.Money(r.UnitPriceAmount, row.Currency),
            new PlantSafety.SharedKernel.Primitives.ValueTypes.Money(r.Amount, row.Currency))).ToList();

        return Invoice.Rehydrate(
            InvoiceId.FromGuid(row.InvoiceId),
            TenantId.FromGuid(row.TenantId),
            new BillingPeriod(row.PeriodYear, row.PeriodMonth),
            lineItems,
            new PlantSafety.SharedKernel.Primitives.ValueTypes.Money(row.SubtotalAmount, row.Currency),
            new PlantSafety.SharedKernel.Primitives.ValueTypes.Money(row.TotalDueAmount, row.Currency),
            Domain.Billing.BillingEngine.Enums.InvoiceStatus.From(row.Status),
            PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp.From(row.GeneratedAtUtc),
            row.IssuedAtUtc.HasValue ? PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp.From(row.IssuedAtUtc.Value) : null,
            row.PaidAtUtc.HasValue ? PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp.From(row.PaidAtUtc.Value) : null,
            row.PaymentProcessorTransactionId);
    }

    private sealed record InvoiceRow
    {
        public required Guid InvoiceId { get; init; }
        public required Guid TenantId { get; init; }
        public required int PeriodYear { get; init; }
        public required int PeriodMonth { get; init; }
        public required string Currency { get; init; }
        public required string LineItemsJson { get; init; }
        public required decimal SubtotalAmount { get; init; }
        public required decimal TotalDueAmount { get; init; }
        public required string Status { get; init; }
        public required DateTime GeneratedAtUtc { get; init; }
        public DateTime? IssuedAtUtc { get; init; }
        public DateTime? PaidAtUtc { get; init; }
        public string? PaymentProcessorTransactionId { get; init; }
    }

    private sealed record LineItemRow(string Type, string Description, decimal Quantity, decimal UnitPriceAmount, decimal Amount);
}
