// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Enterprise.EventMesh
//  Layer:          Tests (Unit)
//  File:           ErpWorkOrderStatusSyncInboundTranslatorTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Verifies the ERP -> Platform inbound translator: CanHandle scoping (SourceSystem +
//                  EventType), successful translation into UpdateWorkOrderStatusCommand dispatched
//                  via ISender, and graceful failure on a malformed payload. Isolated, deterministic,
//                  no I/O.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Enterprise.EventMesh.Translators;
using PlantSafety.Application.Operational.WorkOrders.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Tests.Enterprise.EventMesh;

public sealed class ErpWorkOrderStatusSyncInboundTranslatorTests : UnitTestBase
{
    private readonly ISender _sender = Substitute.For<ISender>();

    private ErpWorkOrderStatusSyncInboundTranslator Sut() => new(_sender);

    private static EnterpriseEventEnvelope EnvelopeFor(EnterpriseSourceSystem source, string eventType, object payload) =>
        EnterpriseEventEnvelope.Create(eventType, 1, source, FixedUtcNow,
            TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, TestUserId.Value, payload);

    [Fact]
    public void CanHandle_ForErpWorkOrderStatusSyncEvent_ReturnsTrue()
    {
        var envelope = EnvelopeFor(EnterpriseSourceSystem.Erp, ErpWorkOrderStatusSyncInboundTranslator.HandledEventType, new { workOrderId = Guid.NewGuid(), targetStatus = "InProgress" });

        Sut().CanHandle(envelope).Should().BeTrue();
    }

    [Fact]
    public void CanHandle_ForDifferentSourceSystem_ReturnsFalse()
    {
        var envelope = EnvelopeFor(EnterpriseSourceSystem.Mes, ErpWorkOrderStatusSyncInboundTranslator.HandledEventType, new { workOrderId = Guid.NewGuid(), targetStatus = "InProgress" });

        Sut().CanHandle(envelope).Should().BeFalse();
    }

    [Fact]
    public async Task TranslateAndDispatchAsync_WithValidPayload_DispatchesUpdateWorkOrderStatusCommand()
    {
        var workOrderId = Guid.NewGuid();
        var envelope = EnvelopeFor(EnterpriseSourceSystem.Erp, ErpWorkOrderStatusSyncInboundTranslator.HandledEventType,
            new { workOrderId, targetStatus = "InProgress", reason = "ERP sync" });

        _sender.Send(Arg.Any<UpdateWorkOrderStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<WorkOrderStatusResult>.Success(new WorkOrderStatusResult(workOrderId, "InProgress")));

        var result = await Sut().TranslateAndDispatchAsync(envelope, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _sender.Received(1).Send(
            Arg.Is<UpdateWorkOrderStatusCommand>(c => c.WorkOrderId == workOrderId && c.TargetStatus == "InProgress" && c.TenantId == TestTenantId.Value),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TranslateAndDispatchAsync_WithMissingWorkOrderId_ReturnsFailureWithoutDispatching()
    {
        var envelope = EnvelopeFor(EnterpriseSourceSystem.Erp, ErpWorkOrderStatusSyncInboundTranslator.HandledEventType,
            new { targetStatus = "InProgress" });

        var result = await Sut().TranslateAndDispatchAsync(envelope, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _sender.DidNotReceive().Send(Arg.Any<UpdateWorkOrderStatusCommand>(), Arg.Any<CancellationToken>());
    }
}
