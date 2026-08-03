// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh.Erp
//  Layer:          Infrastructure
//  File:           FinancialRecordApprovedEnterpriseEventMapper.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Maps the real, internal FinancialRecordApprovedEvent onto the canonical
//                  cross-system EnterpriseEventEnvelope for outbound financial-posting propagation
//                  to ERP. This is the genuine "financial-impact" trigger event: the real Financials
//                  module's FinancialRecord aggregate lifecycle transitions Draft -&gt; Submitted -&gt;
//                  Approved, and Approved is the point at which a financial record is ready to post
//                  to the external ERP's GL/cost-center ledger (documented deviation from the
//                  originating task brief's example scenario of "completed work order with recorded
//                  cost" -- WorkOrderCompletedEvent carries no cost field and Financials.RecordCost
//                  has no real domain event yet; FinancialRecordApprovedEvent is the actual,
//                  already-built financial-impact event this hook fires on). EventType:
//                  "plantsafety.financialrecord.approved.v1". Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;
using PlantSafety.Application.Financials.Models;
using PlantSafety.Domain.Financials.Events;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Erp;

/// <summary>Maps FinancialRecord lifecycle domain events onto <see cref="EnterpriseEventEnvelope"/> for the Enterprise Event Mesh.</summary>
public static class FinancialRecordApprovedEnterpriseEventMapper
{
    /// <summary>The canonical, namespaced EventType for this envelope.</summary>
    public const string ApprovedEventType = "plantsafety.financialrecord.approved.v1";

    /// <summary>The current schema version this mapper produces.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// Maps a <see cref="FinancialRecordApprovedEvent"/>, enriched with the approved record's detail
    /// (<paramref name="record"/>) and, when configured, its <paramref name="costCenterMapping"/>,
    /// onto an outbound <see cref="EnterpriseEventEnvelope"/> for ERP financial posting.
    /// </summary>
    public static EnterpriseEventEnvelope ToEnvelope(
        FinancialRecordApprovedEvent domainEvent,
        FinancialRecordDetailDto record,
        CostCenterMapping? costCenterMapping,
        Guid tenantId,
        Guid actorId) =>
        EnterpriseEventEnvelope.Create(
            eventType: ApprovedEventType,
            schemaVersion: CurrentSchemaVersion,
            sourceSystem: EnterpriseSourceSystem.Platform,
            occurredAtUtc: domainEvent.ApprovedAtUtc.Value,
            correlationId: domainEvent.CorrelationId.Value,
            causationId: domainEvent.EventId,
            tenantId: tenantId,
            actorId: actorId == Guid.Empty ? null : actorId,
            payload: new FinancialRecordApprovedEnterprisePayload(
                domainEvent.FinancialRecordId,
                record.SiteId,
                record.Category,
                record.Amount,
                costCenterMapping?.ExternalCostCenterCode,
                domainEvent.ApprovedBy.Value,
                domainEvent.ApprovedAtUtc.Value));

    /// <summary>Payload shape for <see cref="ApprovedEventType"/> SchemaVersion 1.</summary>
    /// <param name="FinancialRecordId">Identity of the approved FinancialRecord.</param>
    /// <param name="SiteId">Site the record is scoped to, if any.</param>
    /// <param name="Category">Financial category (e.g. Budget, Cost, Forecast).</param>
    /// <param name="Amount">Monetary amount to post.</param>
    /// <param name="ExternalCostCenterCode">The ERP cost center code to post against, if a mapping is configured for this Tenant/Site.</param>
    /// <param name="ApprovedBy">Identity of the user who approved the record.</param>
    /// <param name="ApprovedAtUtc">UTC timestamp of approval.</param>
    public sealed record FinancialRecordApprovedEnterprisePayload(
        Guid FinancialRecordId,
        Guid? SiteId,
        string Category,
        decimal Amount,
        string? ExternalCostCenterCode,
        Guid ApprovedBy,
        DateTime ApprovedAtUtc);
}
