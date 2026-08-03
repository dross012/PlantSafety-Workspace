// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Enterprise.EventMesh
//  Layer:          Tests (Unit)
//  File:           IngestEnterpriseInboundEventCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2/§3.3, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Verifies the inbound ingest handler resolves the matching translator, gracefully
//                  ignores (does not fail) an EventType with no registered translator, and runs the
//                  envelope through EventUpgradePipeline before translation. Isolated, deterministic,
//                  no I/O.
// ============================================================================
using PlantSafety.Application.Enterprise.EventMesh.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Tests.Enterprise.EventMesh;

public sealed class IngestEnterpriseInboundEventCommandHandlerTests : UnitTestBase
{
    private static EnterpriseEventEnvelope EnvelopeFor(string eventType) =>
        EnterpriseEventEnvelope.Create(eventType, 1, EnterpriseSourceSystem.Erp, FixedUtcNow,
            TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, TestUserId.Value, new { workOrderId = Guid.NewGuid() });

    private static IngestEnterpriseInboundEventCommand CommandFor(EnterpriseEventEnvelope envelope) =>
        new(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, envelope);

    [Fact]
    public async Task Handle_WithRecognisedEventType_DispatchesViaMatchingTranslator()
    {
        var envelope = EnvelopeFor("erp.workorder.statuschanged.v1");
        var translator = Substitute.For<IEnterpriseInboundEventTranslator>();
        translator.CanHandle(Arg.Any<EnterpriseEventEnvelope>()).Returns(true);
        translator.TranslateAndDispatchAsync(Arg.Any<EnterpriseEventEnvelope>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        var pipeline = new EventUpgradePipeline([], NullLogger<EventUpgradePipeline>());
        var sut = new IngestEnterpriseInboundEventCommandHandler([translator], pipeline, NullLogger<IngestEnterpriseInboundEventCommandHandler>());

        var result = await sut.Handle(CommandFor(envelope), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Handled.Should().BeTrue();
        await translator.Received(1).TranslateAndDispatchAsync(Arg.Any<EnterpriseEventEnvelope>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognisedEventType_GracefullyIgnoresWithoutFailure()
    {
        var envelope = EnvelopeFor("cmms.futureeventtype.v1");
        var pipeline = new EventUpgradePipeline([], NullLogger<EventUpgradePipeline>());
        var sut = new IngestEnterpriseInboundEventCommandHandler([], pipeline, NullLogger<IngestEnterpriseInboundEventCommandHandler>());

        var result = await sut.Handle(CommandFor(envelope), CancellationToken.None);

        result.IsSuccess.Should().BeTrue("an EventType this platform version does not yet understand must be ignored gracefully, not fail the inbound adapter");
        result.Value.Handled.Should().BeFalse();
    }
}
