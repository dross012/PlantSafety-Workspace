// AUTHOR: PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command persisting a UsageMeteringSnapshot for a tenant/billing period, so a
//   later billing run (GenerateInvoiceCommand) can compute overage deterministically without
//   re-querying live, possibly-aged-out data sources.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.Metering.Commands;

/// <summary>Captures and persists a UsageMeteringSnapshot for the given tenant/billing period, composing every existing metering data source.</summary>
/// <param name="TenantId">Tenant to capture usage metering for.</param>
/// <param name="PeriodYear">Calendar year of the billing period (UTC).</param>
/// <param name="PeriodMonth">Calendar month of the billing period (UTC), 1-12.</param>
public sealed record CaptureUsageMeteringSnapshotCommand(Guid TenantId, int PeriodYear, int PeriodMonth) : IRequest<Result<Guid>>;
