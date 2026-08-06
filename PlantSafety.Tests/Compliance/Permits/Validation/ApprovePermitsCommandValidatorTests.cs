// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Permits.Validation
//  Layer:          Tests (Unit)
//  File:           ApprovePermitsCommandValidatorTests.cs
//  Created:        2026-08-05
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ApprovePermitsCommandValidator's cross-domain
//                  PermitRequiresTrainingRule check. Substitutes ISender behind both
//                  PermitComplianceFactProvider and WorkerTrainingFactsProvider so no real
//                  queries/SQL execute. Isolated, deterministic, <50ms per test.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Commands;
using PlantSafety.Application.Compliance.Permits.Models;
using PlantSafety.Application.Compliance.Permits.Queries;
using PlantSafety.Application.Compliance.Permits.Validation;
using PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;
using PlantSafety.Application.Operational.Training.Models;
using PlantSafety.Application.Operational.Training.Queries;

namespace PlantSafety.Tests.Compliance.Permits.Validation;

public sealed class ApprovePermitsCommandValidatorTests : UnitTestBase
{
    private readonly ISender _sender = Substitute.For<ISender>();

    private ApprovePermitsCommandValidator Sut() =>
        new(new PermitComplianceFactProvider(_sender), new WorkerTrainingFactsProvider(_sender));

    private static PermitsItemDto PermitFor(Guid requesterId) => new()
    {
        PermitsId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        SiteId = null,
        Status = "Created",
        PermitType = "HotWork",
        PermitNumber = "PTW-2026-0001",
        WorkDescription = "Welding on Line 3",
        Location = "Line 3",
        HazardsIdentified = "Fire, fumes",
        RequestedByUserId = requesterId,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

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

    private ApprovePermitsCommand CommandFor(Guid permitsId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        PermitsId: permitsId,
        ApprovedByUserId: TestUserId.Value,
        ValidityHours: 8);

    [Fact]
    public async Task Validate_RequesterHasTrainingRecord_Passes()
    {
        var requesterId = Guid.NewGuid();
        var permit = PermitFor(requesterId);

        _sender.Send(Arg.Any<GetPermitsByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PermitsItemDto>.Success(permit));
        _sender.Send(Arg.Any<GetTrainingQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrainingQueryResult>.Success(new TrainingQueryResult(
                TestTenantId.Value, null, FixedUtcNow, [TrainingRecordFor(requesterId)])));

        var result = await Sut().ValidateAsync(CommandFor(permit.PermitsId));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_RequesterHasNoTrainingRecord_Fails()
    {
        var requesterId = Guid.NewGuid();
        var permit = PermitFor(requesterId);

        _sender.Send(Arg.Any<GetPermitsByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PermitsItemDto>.Success(permit));
        _sender.Send(Arg.Any<GetTrainingQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrainingQueryResult>.Success(new TrainingQueryResult(
                TestTenantId.Value, null, FixedUtcNow, [])));

        var result = await Sut().ValidateAsync(CommandFor(permit.PermitsId));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "Safety.PermitRequesterHasNoTrainingRecord");
    }

    [Fact]
    public async Task Validate_PermitNotFound_DoesNotFailOnTrainingCheck()
    {
        // Not-found is the command handler's concern, not this cross-domain check's -- the
        // validator must not surface a misleading training-related error for an unrelated failure.
        _sender.Send(Arg.Any<GetPermitsByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PermitsItemDto>.Failure(new ApplicationError("Permits.NotFound", "Not found.", Guid.NewGuid())));

        var result = await Sut().ValidateAsync(CommandFor(Guid.NewGuid()));

        result.Errors.Should().NotContain(e => e.ErrorCode == "Safety.PermitRequesterHasNoTrainingRecord");
    }

    [Fact]
    public async Task Validate_PermitHasNoRecordedRequester_TreatsAsMissingTrainingAndFails()
    {
        var permit = PermitFor(Guid.NewGuid()) with { RequestedByUserId = null };

        _sender.Send(Arg.Any<GetPermitsByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PermitsItemDto>.Success(permit));

        var result = await Sut().ValidateAsync(CommandFor(permit.PermitsId));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "Safety.PermitRequesterHasNoTrainingRecord");
    }
}
