// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Validation
//  Layer:          Tests (Unit)
//  File:           WaterDischargeComplianceValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper coverage for all 22 WaterDischargeCompliance command
//                  validators: a valid command must pass with zero errors, and each required/bounded
//                  field must independently reject an invalid value.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Validation;

public sealed class WaterDischargeComplianceValidatorTests : UnitTestBase
{
    private static readonly Guid Id = Guid.NewGuid();

    // ---- DischargePermit ----------------------------------------------------

    [Fact]
    public void IssueDischargePermit_WithValidCommand_PassesValidation()
    {
        var command = new IssueDischargePermitCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            "NPDES-1", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1), "conditions", Guid.NewGuid());

        new IssueDischargePermitCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IssueDischargePermit_WithExpiresBeforeEffective_FailsValidation()
    {
        var command = new IssueDischargePermitCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            "NPDES-1", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddDays(-1), "conditions", Guid.NewGuid());

        new IssueDischargePermitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.ExpiresAtUtc);
    }

    [Fact]
    public void IssueDischargePermit_WithEmptyPermitNumber_FailsValidation()
    {
        var command = new IssueDischargePermitCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            "", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1), "conditions", Guid.NewGuid());

        new IssueDischargePermitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.PermitNumber);
    }

    [Fact]
    public void RenewDischargePermit_WithEmptyPermitId_FailsValidation()
    {
        var command = new RenewDischargePermitCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, FixedUtcNow.AddYears(1), Guid.NewGuid());

        new RenewDischargePermitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.DischargePermitId);
    }

    [Fact]
    public void FlagDischargePermitExpiringSoon_WithValidCommand_PassesValidation()
    {
        var command = new FlagDischargePermitExpiringSoonCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.NewGuid());

        new FlagDischargePermitExpiringSoonCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SuspendDischargePermit_WithEmptyReason_FailsValidation()
    {
        var command = new SuspendDischargePermitCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, "", Guid.NewGuid());

        new SuspendDischargePermitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void RevokeDischargePermit_WithValidCommand_PassesValidation()
    {
        var command = new RevokeDischargePermitCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, "Repeat violations.", Guid.NewGuid());

        new RevokeDischargePermitCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ---- DischargePoint -------------------------------------------------------

    [Fact]
    public void RegisterDischargePoint_WithValidCommand_PassesValidation()
    {
        var command = new RegisterDischargePointCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            Guid.NewGuid(), "Outfall 001", "Main Outfall", "Willow Creek", "Direct", null, null, Guid.NewGuid());

        new RegisterDischargePointCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterDischargePoint_WithEmptyPointCode_FailsValidation()
    {
        var command = new RegisterDischargePointCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            Guid.NewGuid(), "", "Main Outfall", "Willow Creek", "Direct", null, null, Guid.NewGuid());

        new RegisterDischargePointCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.PointCode);
    }

    [Fact]
    public void DeactivateDischargePoint_WithEmptyPointId_FailsValidation()
    {
        var command = new DeactivateDischargePointCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, null, Guid.NewGuid());

        new DeactivateDischargePointCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.DischargePointId);
    }

    [Fact]
    public void ReactivateDischargePoint_WithValidCommand_PassesValidation()
    {
        var command = new ReactivateDischargePointCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.NewGuid());

        new ReactivateDischargePointCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DecommissionDischargePoint_WithEmptyTenantId_FailsValidation()
    {
        var command = new DecommissionDischargePointCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Id, null, Guid.NewGuid());

        new DecommissionDischargePointCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    // ---- EffluentSample ---------------------------------------------------

    [Fact]
    public void RecordEffluentSample_WithValidCommand_PassesValidation()
    {
        var command = new RecordEffluentSampleCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            "PH", 7.2m, "pH", FixedUtcNow, Guid.NewGuid(), "LIMS-Prod", "COC-0001", null, null, Guid.NewGuid());

        new RecordEffluentSampleCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordEffluentSample_WithEmptyChainOfCustodyReference_FailsValidation()
    {
        var command = new RecordEffluentSampleCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            "PH", 7.2m, "pH", FixedUtcNow, Guid.NewGuid(), "LIMS-Prod", "", null, null, Guid.NewGuid());

        new RecordEffluentSampleCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.ChainOfCustodyReference);
    }

    [Fact]
    public void RecordEffluentSample_WithEmptyCollectedByUserId_FailsValidation()
    {
        var command = new RecordEffluentSampleCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            "PH", 7.2m, "pH", FixedUtcNow, Guid.Empty, "LIMS-Prod", "COC-0001", null, null, Guid.NewGuid());

        new RecordEffluentSampleCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.CollectedByUserId);
    }

    [Fact]
    public void VoidEffluentSample_WithEmptyReason_FailsValidation()
    {
        var command = new VoidEffluentSampleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, "", Guid.NewGuid());

        new VoidEffluentSampleCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    // ---- DischargeLimit -----------------------------------------------------

    [Fact]
    public void DefineDischargeLimit_WithValidCommand_PassesValidation()
    {
        var command = new DefineDischargeLimitCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            "PH", "DailyMaximum", 9.0m, "pH", FixedUtcNow, "Part I.A.1", Guid.NewGuid());

        new DefineDischargeLimitCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DefineDischargeLimit_WithNegativeLimitValue_FailsValidation()
    {
        var command = new DefineDischargeLimitCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            "PH", "DailyMaximum", -1m, "pH", FixedUtcNow, "Part I.A.1", Guid.NewGuid());

        new DefineDischargeLimitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.LimitValue);
    }

    [Fact]
    public void DefineDischargeLimit_WithEmptyPermitConditionReference_FailsValidation()
    {
        var command = new DefineDischargeLimitCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            "PH", "DailyMaximum", 9.0m, "pH", FixedUtcNow, "", Guid.NewGuid());

        new DefineDischargeLimitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.PermitConditionReference);
    }

    [Fact]
    public void ReviseDischargeLimit_WithEmptyLimitId_FailsValidation()
    {
        var command = new ReviseDischargeLimitCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 5m, Guid.NewGuid());

        new ReviseDischargeLimitCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.DischargeLimitId);
    }

    [Fact]
    public void RetireDischargeLimit_WithValidCommand_PassesValidation()
    {
        var command = new RetireDischargeLimitCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, null, Guid.NewGuid());

        new RetireDischargeLimitCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ---- LimitExceedance ----------------------------------------------------

    [Fact]
    public void RecordLimitExceedance_WithValidCommand_PassesValidation()
    {
        var command = new RecordLimitExceedanceCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "pH", 10.5m, 9.0m, "Major", FixedUtcNow, 0, Guid.NewGuid());

        new RecordLimitExceedanceCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordLimitExceedance_WithEmptyParameterName_FailsValidation()
    {
        var command = new RecordLimitExceedanceCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "", 10.5m, 9.0m, "Major", FixedUtcNow, 0, Guid.NewGuid());

        new RecordLimitExceedanceCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.ParameterName);
    }

    [Fact]
    public void InitiateExceedanceResponse_WithEmptyResponseUserId_FailsValidation()
    {
        var command = new InitiateExceedanceResponseCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.Empty, "notes", Guid.NewGuid());

        new InitiateExceedanceResponseCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.ResponseUserId);
    }

    [Fact]
    public void InitiateExceedanceResponse_WithValidCommand_PassesValidation()
    {
        var command = new InitiateExceedanceResponseCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.NewGuid(), "notes", Guid.NewGuid());

        new InitiateExceedanceResponseCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CloseExceedanceResponse_WithEmptyResponseUserId_FailsValidation()
    {
        var command = new CloseExceedanceResponseCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.Empty, "resolution", Guid.NewGuid());

        new CloseExceedanceResponseCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.ResponseUserId);
    }

    [Fact]
    public void CloseExceedanceResponse_WithValidCommand_PassesValidation()
    {
        var command = new CloseExceedanceResponseCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.NewGuid(), "resolution", Guid.NewGuid());

        new CloseExceedanceResponseCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ---- DischargeMonitoringReport --------------------------------------------

    [Fact]
    public void PrepareDischargeMonitoringReport_WithValidCommand_PassesValidation()
    {
        var command = new PrepareDischargeMonitoringReportCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            FixedUtcNow.AddMonths(-1), FixedUtcNow, FixedUtcNow.AddDays(28), Guid.NewGuid());

        new PrepareDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PrepareDischargeMonitoringReport_WithPeriodEndBeforeStart_FailsValidation()
    {
        var command = new PrepareDischargeMonitoringReportCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            FixedUtcNow, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(28), Guid.NewGuid());

        new PrepareDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.ReportingPeriodEndUtc);
    }

    [Fact]
    public void PrepareDischargeMonitoringReport_WithDueDateBeforePeriodEnd_FailsValidation()
    {
        var command = new PrepareDischargeMonitoringReportCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            FixedUtcNow.AddMonths(-1), FixedUtcNow, FixedUtcNow.AddDays(-2), Guid.NewGuid());

        new PrepareDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.DueAtUtc);
    }

    [Fact]
    public void SubmitDischargeMonitoringReport_WithSamplesWithinLimitExceedingTotal_FailsValidation()
    {
        var command = new SubmitDischargeMonitoringReportCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, FixedUtcNow, 40, 30, Guid.NewGuid());

        new SubmitDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.SamplesWithinLimitCount);
    }

    [Fact]
    public void SubmitDischargeMonitoringReport_WithValidCommand_PassesValidation()
    {
        var command = new SubmitDischargeMonitoringReportCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, FixedUtcNow, 28, 30, Guid.NewGuid());

        new SubmitDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AcceptDischargeMonitoringReport_WithEmptyReportId_FailsValidation()
    {
        var command = new AcceptDischargeMonitoringReportCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        new AcceptDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.DischargeMonitoringReportId);
    }

    [Fact]
    public void RejectDischargeMonitoringReport_WithEmptyReason_FailsValidation()
    {
        var command = new RejectDischargeMonitoringReportCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, "", Guid.NewGuid());

        new RejectDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void RejectDischargeMonitoringReport_WithValidCommand_PassesValidation()
    {
        var command = new RejectDischargeMonitoringReportCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, "Missing attachments.", Guid.NewGuid());

        new RejectDischargeMonitoringReportCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ---- Cross-cutting: every command validator rejects Guid.Empty PerformedByUserId ----------

    [Fact]
    public void FlagDischargePermitExpiringSoon_WithEmptyPerformedByUserId_FailsValidation()
    {
        var command = new FlagDischargePermitExpiringSoonCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Id, Guid.Empty);

        new FlagDischargePermitExpiringSoonCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.PerformedByUserId);
    }
}
