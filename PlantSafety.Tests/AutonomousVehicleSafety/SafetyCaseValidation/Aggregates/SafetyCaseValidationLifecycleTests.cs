// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Tests (Unit)
//  File:           SafetyCaseValidationLifecycleTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the SafetyCaseValidation aggregate root: Create
//                  (Submit)/Validate/Reject/Expire transitions and their guard rules, including the
//                  expiry-date computation from RevalidationCadenceDays. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Events;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Aggregates.SafetyCaseValidation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.SafetyCaseValidation.Aggregates;

public sealed class SafetyCaseValidationLifecycleTests : UnitTestBase
{
    private static Agg SubmitCase(int revalidationCadenceDays = 365) => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        autonomousVehicleAssetId: Guid.NewGuid(),
        standardReference: SafetyStandardReference.AnsiRia1508,
        documentVersion: "v1.0",
        revalidationCadenceDays: revalidationCadenceDays);

    [Fact]
    public void Create_ShouldInitializeWithPendingReviewStatus_AndRaiseSubmittedEvent()
    {
        var safetyCase = SubmitCase();

        safetyCase.Status.Should().Be(SafetyCaseValidationStatus.PendingReview);
        safetyCase.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SafetyCaseSubmittedEvent>();
    }

    [Fact]
    public void Validate_WhenPendingReview_ShouldComputeExpiryFromCadence_AndTransitionToValidated()
    {
        var safetyCase = SubmitCase(revalidationCadenceDays: 90);
        var validatedBy = UserId.FromGuid(Guid.NewGuid());

        safetyCase.Validate(validatedBy, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        safetyCase.Status.Should().Be(SafetyCaseValidationStatus.Validated);
        safetyCase.ValidatedByUserId.Should().Be(validatedBy);
        safetyCase.ExpiresAtUtc!.Value.Value.Should().Be(FixedUtcNow.AddDays(90));
    }

    [Fact]
    public void Validate_WhenAlreadyValidated_ShouldThrowInvalidOperationException()
    {
        var safetyCase = SubmitCase();
        safetyCase.Validate(UserId.FromGuid(Guid.NewGuid()), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => safetyCase.Validate(UserId.FromGuid(Guid.NewGuid()), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_WhenPendingReview_ShouldTransitionToRejected_AndRecordReason()
    {
        var safetyCase = SubmitCase();

        safetyCase.Reject("Insufficient hazard analysis coverage", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        safetyCase.Status.Should().Be(SafetyCaseValidationStatus.Rejected);
        safetyCase.RejectionReason.Should().Be("Insufficient hazard analysis coverage");
    }

    [Fact]
    public void Reject_WhenAlreadyValidated_ShouldThrowInvalidOperationException()
    {
        var safetyCase = SubmitCase();
        safetyCase.Validate(UserId.FromGuid(Guid.NewGuid()), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => safetyCase.Reject("too late", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Expire_WhenValidated_ShouldTransitionToExpired()
    {
        var safetyCase = SubmitCase();
        safetyCase.Validate(UserId.FromGuid(Guid.NewGuid()), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        safetyCase.Expire(UtcTimestamp.From(FixedUtcNow.AddDays(400)), TestActor, TestCorrelationId, TestCausationId);

        safetyCase.Status.Should().Be(SafetyCaseValidationStatus.Expired);
    }

    [Fact]
    public void Expire_WhenPendingReview_ShouldThrowInvalidOperationException()
    {
        var safetyCase = SubmitCase();

        var act = () => safetyCase.Expire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
