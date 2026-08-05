// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityMeetingRecord.Validation
//  Layer:          Tests (Unit)
//  File:           RecordCommunityMeetingCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordCommunityMeetingCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.Validation;

namespace PlantSafety.Tests.CommunitySafety.CommunityMeetingRecord.Validation;

public sealed class RecordCommunityMeetingCommandValidatorTests : UnitTestBase
{
    private readonly RecordCommunityMeetingCommandValidator _validator = new();

    private static RecordCommunityMeetingCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
        "Q3 Community Advisory Panel", FixedUtcNow, "Community Center, Room B", 12,
        "Reviewed Q3 flare activity.", "Publish schedule.", TestUserId.Value, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyMeetingTitle_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { MeetingTitle = "" });
        result.ShouldHaveValidationErrorFor(c => c.MeetingTitle);
    }

    [Fact]
    public void EmptyLocation_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Location = "" });
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }

    [Fact]
    public void NegativeAttendeeCount_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AttendeeCount = -1 });
        result.ShouldHaveValidationErrorFor(c => c.AttendeeCount);
    }

    [Fact]
    public void ZeroAttendeeCount_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AttendeeCount = 0 });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTopicsSummary_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { TopicsSummary = "" });
        result.ShouldHaveValidationErrorFor(c => c.TopicsSummary);
    }

    [Fact]
    public void EmptyRecordedByUserId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { RecordedByUserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.RecordedByUserId);
    }
}
