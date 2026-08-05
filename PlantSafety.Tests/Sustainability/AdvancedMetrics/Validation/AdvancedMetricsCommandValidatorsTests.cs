// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Validation
//  Layer:          Tests (Unit)
//  File:           AdvancedMetricsCommandValidatorsTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper tests for all six Sustainability.AdvancedMetrics
//                  command validators: RecordCarbonEmission, RecordEnergyIntensityMetric,
//                  RecordWaterIntensityMetric, RecordWasteDiversion, SetSustainabilityTarget,
//                  GenerateEsgDisclosureReport. Isolated, deterministic, <50ms per test. No I/O,
//                  no network.
// ============================================================================
using PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Validation;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Validation;

public sealed class RecordCarbonEmissionCommandValidatorTests : UnitTestBase
{
    private readonly RecordCarbonEmissionCommandValidator _validator = new();

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private RecordCarbonEmissionCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd, Scope: "Scope1",
        ActivityDescription: "Boiler House 2", EmissionsMetricTonsCo2e: 12.5m, MethodologyVersion: "GHG-Protocol-2026.1",
        ActivityAtUtc: FixedUtcNow, RequesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEndBeforeStart_HasErrorForReportingPeriodEndUtc()
        => _validator.TestValidate(ValidCommand() with { ReportingPeriodEndUtc = PeriodStart.AddDays(-1) })
            .ShouldHaveValidationErrorFor(x => x.ReportingPeriodEndUtc);

    [Fact]
    public void Validate_WithUnrecognizedScope_HasErrorForScope()
        => _validator.TestValidate(ValidCommand() with { Scope = "Scope99" })
            .ShouldHaveValidationErrorFor(x => x.Scope);

    [Fact]
    public void Validate_WithNegativeEmissions_HasErrorForEmissionsMetricTonsCo2e()
        => _validator.TestValidate(ValidCommand() with { EmissionsMetricTonsCo2e = -1m })
            .ShouldHaveValidationErrorFor(x => x.EmissionsMetricTonsCo2e);

    [Fact]
    public void Validate_WithBlankMethodologyVersion_HasErrorForMethodologyVersion()
        => _validator.TestValidate(ValidCommand() with { MethodologyVersion = "" })
            .ShouldHaveValidationErrorFor(x => x.MethodologyVersion);

    [Fact]
    public void Validate_WithFutureActivityDate_HasErrorForActivityAtUtc()
        // The rule compares against the real wall-clock DateTime.UtcNow (not the frozen
        // UnitTestBase.FixedUtcNow), so the "future" boundary must be anchored to it too.
        => _validator.TestValidate(ValidCommand() with { ActivityAtUtc = DateTime.UtcNow.AddYears(1) })
            .ShouldHaveValidationErrorFor(x => x.ActivityAtUtc);
}

public sealed class RecordEnergyIntensityMetricCommandValidatorTests : UnitTestBase
{
    private readonly RecordEnergyIntensityMetricCommandValidator _validator = new();

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private RecordEnergyIntensityMetricCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd,
        EnergyConsumedKwh: 1000m, OutputUnits: 400m, MethodologyVersion: "Method-1",
        ReadingAtUtc: FixedUtcNow, RequesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithNegativeConsumption_HasErrorForEnergyConsumedKwh()
        => _validator.TestValidate(ValidCommand() with { EnergyConsumedKwh = -1m })
            .ShouldHaveValidationErrorFor(x => x.EnergyConsumedKwh);

    [Fact]
    public void Validate_WithZeroOutputUnits_HasErrorForOutputUnits()
        => _validator.TestValidate(ValidCommand() with { OutputUnits = 0m })
            .ShouldHaveValidationErrorFor(x => x.OutputUnits);

    [Fact]
    public void Validate_WithEndBeforeStart_HasErrorForReportingPeriodEndUtc()
        => _validator.TestValidate(ValidCommand() with { ReportingPeriodEndUtc = PeriodStart.AddDays(-1) })
            .ShouldHaveValidationErrorFor(x => x.ReportingPeriodEndUtc);
}

public sealed class RecordWaterIntensityMetricCommandValidatorTests : UnitTestBase
{
    private readonly RecordWaterIntensityMetricCommandValidator _validator = new();

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private RecordWaterIntensityMetricCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd,
        WaterConsumedCubicMeters: 300m, OutputUnits: 500m, MethodologyVersion: "Method-1",
        ReadingAtUtc: FixedUtcNow, RequesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithNegativeConsumption_HasErrorForWaterConsumedCubicMeters()
        => _validator.TestValidate(ValidCommand() with { WaterConsumedCubicMeters = -1m })
            .ShouldHaveValidationErrorFor(x => x.WaterConsumedCubicMeters);

    [Fact]
    public void Validate_WithZeroOutputUnits_HasErrorForOutputUnits()
        => _validator.TestValidate(ValidCommand() with { OutputUnits = 0m })
            .ShouldHaveValidationErrorFor(x => x.OutputUnits);
}

public sealed class RecordWasteDiversionCommandValidatorTests : UnitTestBase
{
    private readonly RecordWasteDiversionCommandValidator _validator = new();

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private RecordWasteDiversionCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd,
        TotalWasteKg: 1000m, DivertedWasteKg: 750m, MethodologyVersion: "Method-1",
        IntervalEndAtUtc: FixedUtcNow, RequesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithDivertedExceedingTotal_HasErrorForDivertedWasteKg()
        => _validator.TestValidate(ValidCommand() with { DivertedWasteKg = 2000m })
            .ShouldHaveValidationErrorFor(x => x.DivertedWasteKg);

    [Fact]
    public void Validate_WithNegativeTotalWaste_HasErrorForTotalWasteKg()
        => _validator.TestValidate(ValidCommand() with { TotalWasteKg = -1m })
            .ShouldHaveValidationErrorFor(x => x.TotalWasteKg);
}

public sealed class SetSustainabilityTargetCommandValidatorTests : UnitTestBase
{
    private readonly SetSustainabilityTargetCommandValidator _validator = new();

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private SetSustainabilityTargetCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd,
        TargetType: "WasteDiversionRate", BaselineValue: 40m, TargetValue: 75m,
        TargetDateUtc: FixedUtcNow.AddYears(1), RequesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithUnrecognizedTargetType_HasErrorForTargetType()
        => _validator.TestValidate(ValidCommand() with { TargetType = "NotARealTarget" })
            .ShouldHaveValidationErrorFor(x => x.TargetType);

    [Fact]
    public void Validate_WithPastTargetDate_HasErrorForTargetDateUtc()
        => _validator.TestValidate(ValidCommand() with { TargetDateUtc = FixedUtcNow.AddDays(-1) })
            .ShouldHaveValidationErrorFor(x => x.TargetDateUtc);
}

public sealed class GenerateEsgDisclosureReportCommandValidatorTests : UnitTestBase
{
    private readonly GenerateEsgDisclosureReportCommandValidator _validator = new();

    private GenerateEsgDisclosureReportCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        SourceLedgerId: Guid.NewGuid(), Frameworks: "GhgProtocol, Csrd", MethodologyVersion: "GHG-Protocol-2026.1",
        RequesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptySourceLedgerId_HasErrorForSourceLedgerId()
        => _validator.TestValidate(ValidCommand() with { SourceLedgerId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.SourceLedgerId);

    [Fact]
    public void Validate_WithBlankFrameworks_HasErrorForFrameworks()
        => _validator.TestValidate(ValidCommand() with { Frameworks = "" })
            .ShouldHaveValidationErrorFor(x => x.Frameworks);

    [Fact]
    public void Validate_WithBlankMethodologyVersion_HasErrorForMethodologyVersion()
        => _validator.TestValidate(ValidCommand() with { MethodologyVersion = "" })
            .ShouldHaveValidationErrorFor(x => x.MethodologyVersion);
}
