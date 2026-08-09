// ============================================================================
//  Project:        Shared.EmployeeDomain.Tests
//  File:           EmployeeAggregateTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.1 unit tests for EmployeeAggregate: command
//                  correctness, immutable state replacement, safety-score
//                  recomputation, and domain event collection/clearing.
// ============================================================================
using FluentAssertions;

using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Events;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

using Shared.FacilityDomain.Abstractions.Hazards;

using Xunit;

namespace Shared.EmployeeDomain.Tests.Aggregates;

public sealed class EmployeeAggregateTests
{
    [Fact]
    public void CreateEmployee_SetsInitialStateAndRaisesEmployeeCreatedEvent()
    {
        var employeeId = EmployeeId.New();
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(employeeId, "Jane Doe", EmployeeRole.Operator, now);

        aggregate.State.EmployeeId.Should().Be(employeeId);
        aggregate.State.Name.Should().Be("Jane Doe");
        aggregate.State.TrainingProfile.CompletedTrainingCodes.Should().BeEmpty();
        aggregate.State.Certifications.Should().BeEmpty();
        aggregate.State.ExposureRecords.Should().BeEmpty();
        aggregate.State.InjuryProfile.TotalInjuryCount.Should().Be(0);
        aggregate.State.SafetyScore.ScoreValue.Should().Be(100);
        aggregate.State.CreatedAtUtc.Should().Be(now);

        aggregate.DomainEvents.Should().ContainSingle();
        aggregate.DomainEvents[0].Should().BeOfType<EmployeeCreatedEvent>()
            .Which.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void CreateEmployee_BlankName_Throws()
    {
        Action act = () => EmployeeAggregate.CreateEmployee(EmployeeId.New(), "   ", EmployeeRole.Operator, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateEmployeeMetadata_ReplacesStateAndRaisesUpdatedEvent()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);

        aggregate.UpdateEmployeeMetadata("Jane Smith", EmployeeRole.Supervisor, DateTime.UtcNow);

        aggregate.State.Name.Should().Be("Jane Smith");
        aggregate.State.Role.Should().Be(EmployeeRole.Supervisor);
        aggregate.DomainEvents.OfType<EmployeeUpdatedEvent>().Should().Contain(e => e.Kind == EmployeeUpdateKind.Metadata);
    }

    [Fact]
    public void UpdateTrainingProfile_ReplacesCodesAndRecomputesScore()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);

        aggregate.UpdateTrainingProfile(["HAZWOPER", "LOTO"], DateTime.UtcNow);

        aggregate.State.TrainingProfile.CompletedTrainingCodes.Should().BeEquivalentTo(["HAZWOPER", "LOTO"]);
        aggregate.State.SafetyScore.ScoreValue.Should().Be(100);
        aggregate.DomainEvents.Should().ContainSingle(e => e is EmployeeTrainingUpdatedEvent);
    }

    [Fact]
    public void UpdateCertifications_ReplacesCertifications()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);
        var certification = new EmployeeCertification("OSHA-30", "OSHA", DateTime.UtcNow, null);

        aggregate.UpdateCertifications([certification], DateTime.UtcNow);

        aggregate.State.Certifications.Should().ContainSingle().Which.Should().Be(certification);
        aggregate.DomainEvents.Should().ContainSingle(e => e is EmployeeCertificationUpdatedEvent);
    }

    [Fact]
    public void RecordExposure_AppendsRecordAndLowersScore()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);
        var exposure = new EmployeeExposureRecord(HazardType.ChemicalSpill, "Minor solvent exposure", DateTime.UtcNow);

        aggregate.RecordExposure(exposure, DateTime.UtcNow);

        aggregate.State.ExposureRecords.Should().ContainSingle().Which.Should().Be(exposure);
        aggregate.State.SafetyScore.ScoreValue.Should().Be(98);
    }

    [Fact]
    public void RecordInjury_IncrementsInjuryCountAndLowersScore()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);

        aggregate.RecordInjury("Twisted ankle", DateTime.UtcNow);

        aggregate.State.InjuryProfile.TotalInjuryCount.Should().Be(1);
        aggregate.State.InjuryProfile.LastInjuryDescription.Should().Be("Twisted ankle");
        aggregate.State.SafetyScore.ScoreValue.Should().Be(85);
    }

    [Fact]
    public void RecordInjury_BlankDescription_Throws()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);

        Action act = () => aggregate.RecordInjury(" ", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClearDomainEvents_EmptiesTheList()
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);
        aggregate.DomainEvents.Should().NotBeEmpty();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_StartsWithNoDomainEvents()
    {
        EmployeeAggregate original = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);

        EmployeeAggregate rehydrated = EmployeeAggregate.Rehydrate(original.State);

        rehydrated.State.Should().Be(original.State);
        rehydrated.DomainEvents.Should().BeEmpty();
    }
}
