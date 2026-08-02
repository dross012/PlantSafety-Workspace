// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.AccidentManagement
//  Layer:          Tests (Unit)
//  File:           AccidentInvestigationCommandHandlersTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the six Accident investigation-workflow command handlers
//                  (AssignAccidentInvestigatorCommandHandler, RecordAccidentRootCauseCommandHandler,
//                  CreateAccidentCorrectiveActionCommandHandler,
//                  CompleteAccidentCorrectiveActionCommandHandler,
//                  FileAccidentRegulatoryReportCommandHandler,
//                  CloseAccidentInvestigationCommandHandler) with a mocked IAccidentRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AccidentManagement.Commands;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using AccidentAggregate = PlantSafety.Domain.AccidentManagement.Aggregates.Accident;

namespace PlantSafety.Tests.Application.AccidentManagement;

public sealed class AccidentInvestigationCommandHandlersTests : UnitTestBase
{
    private readonly IAccidentRepository _repository = Substitute.For<IAccidentRepository>();

    private static AccidentAggregate NewReportedAccident(AccidentSeverity? severity = null) =>
        AccidentAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, accidentTitle: "Test accident", location: "Loc", severity: severity, ownerId: TestUserId);

    // ---- AssignAccidentInvestigatorCommandHandler -------------------------------

    [Fact]
    public async Task AssignInvestigator_WhenAccidentExists_ShouldTransitionToUnderInvestigation()
    {
        var accident = NewReportedAccident();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new AssignAccidentInvestigatorCommandHandler(_repository, NullLogger<AssignAccidentInvestigatorCommandHandler>());
        var cmd = new AssignAccidentInvestigatorCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("UnderInvestigation");
        await _repository.Received(1).UpdateAsync(accident, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignInvestigator_WhenAccidentNotFound_ShouldReturnFailure()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AccidentAggregate?)null);

        var handler = new AssignAccidentInvestigatorCommandHandler(_repository, NullLogger<AssignAccidentInvestigatorCommandHandler>());
        var cmd = new AssignAccidentInvestigatorCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Accident should surface as 404, not 500");
    }

    // ---- RecordAccidentRootCauseCommandHandler ----------------------------------

    [Fact]
    public async Task RecordRootCause_WhenUnderInvestigation_ShouldTransitionToRootCauseIdentified()
    {
        var accident = NewReportedAccident();
        accident.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new RecordAccidentRootCauseCommandHandler(_repository, NullLogger<RecordAccidentRootCauseCommandHandler>());
        var cmd = new RecordAccidentRootCauseCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, "Mechanical", "Pinch point", TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("RootCauseIdentified");
    }

    [Fact]
    public async Task RecordRootCause_WhenAccidentNotFound_ShouldReturnFailure()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AccidentAggregate?)null);

        var handler = new RecordAccidentRootCauseCommandHandler(_repository, NullLogger<RecordAccidentRootCauseCommandHandler>());
        var cmd = new RecordAccidentRootCauseCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, Guid.NewGuid(), "Mechanical", "Pinch point", TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Accident should surface as 404, not 500");
    }

    // ---- CreateAccidentCorrectiveActionCommandHandler ---------------------------

    private static AccidentAggregate NewRootCauseIdentifiedAccident()
    {
        var accident = NewReportedAccident();
        accident.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        accident.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return accident;
    }

    [Fact]
    public async Task CreateCorrectiveAction_WhenRootCauseIdentified_ShouldSucceed()
    {
        var accident = NewRootCauseIdentifiedAccident();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new CreateAccidentCorrectiveActionCommandHandler(_repository, NullLogger<CreateAccidentCorrectiveActionCommandHandler>());
        var cmd = new CreateAccidentCorrectiveActionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, "EngineeringControls", "Install guard", TestUserId.Value, FixedUtcNow, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("CorrectiveActionsInProgress");
        result.Value.CorrectiveActionId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateCorrectiveAction_WhenAccidentNotFound_ShouldReturnFailure()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AccidentAggregate?)null);

        var handler = new CreateAccidentCorrectiveActionCommandHandler(_repository, NullLogger<CreateAccidentCorrectiveActionCommandHandler>());
        var cmd = new CreateAccidentCorrectiveActionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, Guid.NewGuid(), "EngineeringControls", "Install guard", TestUserId.Value, FixedUtcNow, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Accident should surface as 404, not 500");
    }

    // ---- CompleteAccidentCorrectiveActionCommandHandler -------------------------

    [Fact]
    public async Task CompleteCorrectiveAction_WhenKnownAction_ShouldSucceed()
    {
        var accident = NewRootCauseIdentifiedAccident();
        var action = accident.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new CompleteAccidentCorrectiveActionCommandHandler(_repository, NullLogger<CompleteAccidentCorrectiveActionCommandHandler>());
        var cmd = new CompleteAccidentCorrectiveActionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, action.CorrectiveActionId.Value, "Evidence", TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteCorrectiveAction_WhenActionUnknown_ShouldReturnFailure()
    {
        var accident = NewRootCauseIdentifiedAccident();
        accident.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new CompleteAccidentCorrectiveActionCommandHandler(_repository, NullLogger<CompleteAccidentCorrectiveActionCommandHandler>());
        var cmd = new CompleteAccidentCorrectiveActionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, Guid.NewGuid(), "Evidence", TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.CompleteCorrectiveActionFailed");
    }

    // ---- FileAccidentRegulatoryReportCommandHandler -----------------------------

    [Fact]
    public async Task FileRegulatoryReport_WhenCorrectiveActionsInProgress_ShouldSucceed()
    {
        var accident = NewRootCauseIdentifiedAccident();
        accident.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new FileAccidentRegulatoryReportCommandHandler(_repository, NullLogger<FileAccidentRegulatoryReportCommandHandler>());
        var cmd = new FileAccidentRegulatoryReportCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, "OSHA", "REF-001", TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("RegulatoryFilingComplete");
    }

    // ---- CloseAccidentInvestigationCommandHandler -------------------------------

    [Fact]
    public async Task CloseInvestigation_WhenRegulatoryFilingRequiredButMissing_ShouldReturnFailure()
    {
        var accident = NewReportedAccident(AccidentSeverity.LostTimeInjury);
        accident.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        accident.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        accident.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new CloseAccidentInvestigationCommandHandler(_repository, NullLogger<CloseAccidentInvestigationCommandHandler>());
        var cmd = new CloseAccidentInvestigationCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.CloseInvestigationFailed");
    }

    [Fact]
    public async Task CloseInvestigation_WhenNoRegulatoryFilingRequired_ShouldSucceed()
    {
        var accident = NewReportedAccident(AccidentSeverity.FirstAidOnly);
        accident.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        accident.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        accident.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.AccidentManagement.ValueObjects.Ids.AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(accident);

        var handler = new CloseAccidentInvestigationCommandHandler(_repository, NullLogger<CloseAccidentInvestigationCommandHandler>());
        var cmd = new CloseAccidentInvestigationCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, accident.AccidentId.Value, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
    }
}
