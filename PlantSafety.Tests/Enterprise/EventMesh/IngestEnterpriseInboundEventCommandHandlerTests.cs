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
//
//                  2026-08-03 UPDATE (Phase 9 §1 Reliability Hardening): also verifies a failed
//                  translation/dispatch and an unhandled translator exception both record a
//                  diagnostic dead-letter row via IDeadLetterEventRepository instead of vanishing
//                  after only a log line, and that the exception path is caught and converted to a
//                  Result failure rather than propagating.
// ============================================================================
using PlantSafety.Application.Enterprise.EventMesh.Commands;
using PlantSafety.Application.Messaging.Repositories;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
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
        var deadLetterRepository = Substitute.For<IDeadLetterEventRepository>();
        var sut = new IngestEnterpriseInboundEventCommandHandler(
            [translator], pipeline, deadLetterRepository, NullLogger<IngestEnterpriseInboundEventCommandHandler>());

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
        var deadLetterRepository = Substitute.For<IDeadLetterEventRepository>();
        var sut = new IngestEnterpriseInboundEventCommandHandler(
            [], pipeline, deadLetterRepository, NullLogger<IngestEnterpriseInboundEventCommandHandler>());

        var result = await sut.Handle(CommandFor(envelope), CancellationToken.None);

        result.IsSuccess.Should().BeTrue("an EventType this platform version does not yet understand must be ignored gracefully, not fail the inbound adapter");
        result.Value.Handled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithFailedTranslation_RecordsInboundDeadLetterAndReturnsFailure()
    {
        var envelope = EnvelopeFor("erp.workorder.statuschanged.v1");
        var translator = Substitute.For<IEnterpriseInboundEventTranslator>();
        translator.CanHandle(Arg.Any<EnterpriseEventEnvelope>()).Returns(true);
        translator.TranslateAndDispatchAsync(Arg.Any<EnterpriseEventEnvelope>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Failure(new Error(
                ErrorCode.Create("erp.workorder.unknownstatus"), ErrorType.Validation, "Unrecognised status code 'FOOBAR'.")));

        var pipeline = new EventUpgradePipeline([], NullLogger<EventUpgradePipeline>());
        var deadLetterRepository = Substitute.For<IDeadLetterEventRepository>();
        var sut = new IngestEnterpriseInboundEventCommandHandler(
            [translator], pipeline, deadLetterRepository, NullLogger<IngestEnterpriseInboundEventCommandHandler>());

        var result = await sut.Handle(CommandFor(envelope), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await deadLetterRepository.Received(1).RecordInboundEventMeshFailureAsync(
            Arg.Is<string>(t => t == envelope.EventType), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Is<string>(reason => reason.Contains("FOOBAR")), Arg.Is<string?>(d => d == null),
            Arg.Is<Guid?>(c => c == envelope.CorrelationId), Arg.Is<Guid?>(c => c == envelope.CausationId),
            Arg.Is<string>(s => s == envelope.SourceSystem.ToString()), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTranslatorThrowingException_RecordsInboundDeadLetterAndReturnsFailureWithoutPropagating()
    {
        var envelope = EnvelopeFor("erp.workorder.statuschanged.v1");
        var translator = Substitute.For<IEnterpriseInboundEventTranslator>();
        translator.CanHandle(Arg.Any<EnterpriseEventEnvelope>()).Returns(true);
        translator.TranslateAndDispatchAsync(Arg.Any<EnterpriseEventEnvelope>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<bool>>>(_ => throw new InvalidOperationException("Simulated malformed payload."));

        var pipeline = new EventUpgradePipeline([], NullLogger<EventUpgradePipeline>());
        var deadLetterRepository = Substitute.For<IDeadLetterEventRepository>();
        var sut = new IngestEnterpriseInboundEventCommandHandler(
            [translator], pipeline, deadLetterRepository, NullLogger<IngestEnterpriseInboundEventCommandHandler>());

        var result = await sut.Handle(CommandFor(envelope), CancellationToken.None);

        result.IsFailure.Should().BeTrue("an unhandled translator exception must be converted to a Result failure, never propagate to the caller");
        await deadLetterRepository.Received(1).RecordInboundEventMeshFailureAsync(
            Arg.Is<string>(t => t == envelope.EventType), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Is<string?>(detail => detail != null && detail.Contains("InvalidOperationException")),
            Arg.Is<Guid?>(c => c == envelope.CorrelationId), Arg.Is<Guid?>(c => c == envelope.CausationId),
            Arg.Is<string>(s => s == envelope.SourceSystem.ToString()), Arg.Any<CancellationToken>());
    }
}
