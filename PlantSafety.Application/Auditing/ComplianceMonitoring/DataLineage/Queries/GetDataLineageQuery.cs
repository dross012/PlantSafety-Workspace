// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query reconstructing the cross-system event chain for a given correlation
//                    identifier (Handbook Phase 6 §5.4 Compliance Monitoring -- data lineage:
//                    "which system/event chain produced it"). Composes the existing Outbox and
//                    DeadLetterEvents infrastructure; introduces no new lineage engine.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Queries;

/// <summary>Retrieves the reconstructed data-lineage chain for the given tenant/correlation identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CorrelationId">The correlation identifier whose event chain is being reconstructed.</param>
public sealed record GetDataLineageQuery(
    Guid TenantId,
    Guid CorrelationId) : IRequest<Result<DataLineageQueryResult>>;

/// <summary>Result of <see cref="GetDataLineageQuery"/>.</summary>
/// <param name="CorrelationId">The correlation identifier the chain was reconstructed for.</param>
/// <param name="GeneratedAt">UTC timestamp when this lineage snapshot was generated.</param>
/// <param name="Entries">The reconstructed event chain, oldest first.</param>
public sealed record DataLineageQueryResult(
    Guid CorrelationId,
    DateTime GeneratedAt,
    IReadOnlyList<DataLineageEntryDto> Entries);
