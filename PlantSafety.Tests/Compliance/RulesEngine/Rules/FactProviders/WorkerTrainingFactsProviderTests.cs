// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.RulesEngine.Rules.FactProviders
//  Layer:          Tests (Unit)
//  File:           WorkerTrainingFactsProviderTests.cs
//  Created:        2026-08-05
//  SchemaVersion:  2026.1
//  Description:    Unit tests for WorkerTrainingFactsProvider. Substitutes ISender so no real
//                  GetTrainingQuery/SQL execution occurs -- mirrors
//                  PlantSafety.Tests.Application.GlobalSafetyInsights.Coordination
//                  .CrossSiteIncidentEscalationCommandHandlerTests' ISender substitution style.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;
using PlantSafety.Application.Operational.Training.Models;
using PlantSafety.Application.Operational.Training.Queries;

namespace PlantSafety.Tests.Compliance.RulesEngine.Rules.FactProviders;

public sealed class WorkerTrainingFactsProviderTests : UnitTestBase
{
    private readonly ISender _sender = Substitute.For<ISender>();

    private WorkerTrainingFactsProvider Sut() => new(_sender);

    private static TrainingItemDto TrainingRecordFor(Guid employeeId) => new()
    {
        TrainingRecordId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        SiteId = null,
        EmployeeId = employeeId,
        CourseName = "Confined Space Entry",
        Status = "Completed",
        CompletedAtUtc = FixedUtcNow.AddMonths(-1),
        CertifiedBy = TestUserId.Value,
    };

    [Fact]
    public async Task HasTrainingRecordOnFileAsync_WorkerHasMatchingRecord_ReturnsTrue()
    {
        var workerId = Guid.NewGuid();
        var otherWorkerId = Guid.NewGuid();

        _sender.Send(Arg.Any<GetTrainingQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrainingQueryResult>.Success(new TrainingQueryResult(
                TestTenantId.Value, null, FixedUtcNow,
                [TrainingRecordFor(otherWorkerId), TrainingRecordFor(workerId)])));

        var result = await Sut().HasTrainingRecordOnFileAsync(TestTenantId.Value, null, workerId, FixedUtcNow, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasTrainingRecordOnFileAsync_WorkerHasNoMatchingRecord_ReturnsFalse()
    {
        var workerId = Guid.NewGuid();
        var otherWorkerId = Guid.NewGuid();

        _sender.Send(Arg.Any<GetTrainingQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrainingQueryResult>.Success(new TrainingQueryResult(
                TestTenantId.Value, null, FixedUtcNow,
                [TrainingRecordFor(otherWorkerId)])));

        var result = await Sut().HasTrainingRecordOnFileAsync(TestTenantId.Value, null, workerId, FixedUtcNow, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasTrainingRecordOnFileAsync_NoRecordsAtAll_ReturnsFalse()
    {
        var workerId = Guid.NewGuid();

        _sender.Send(Arg.Any<GetTrainingQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrainingQueryResult>.Success(new TrainingQueryResult(
                TestTenantId.Value, null, FixedUtcNow, [])));

        var result = await Sut().HasTrainingRecordOnFileAsync(TestTenantId.Value, null, workerId, FixedUtcNow, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasTrainingRecordOnFileAsync_QueryFails_FailsClosedReturningFalse()
    {
        var workerId = Guid.NewGuid();

        _sender.Send(Arg.Any<GetTrainingQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrainingQueryResult>.Failure(new ApplicationError("Training.QueryFailed", "boom", Guid.NewGuid())));

        var result = await Sut().HasTrainingRecordOnFileAsync(TestTenantId.Value, null, workerId, FixedUtcNow, CancellationToken.None);

        result.Should().BeFalse();
    }
}
