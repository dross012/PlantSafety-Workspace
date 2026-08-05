// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Compatibility
//  Layer:          Tests (Compatibility)
//  File:           CraneHoistSystemsCompatibilityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Compatibility tests verifying the CraneHoistSystems API surface accepts the
//                  documented request shapes (string-based AssetType enum parsing is
//                  case-insensitive, matching how external callers/clients typically serialize
//                  enums) and that the command contracts embed a stable ApiVersionToken for
//                  backward-compatible client negotiation. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Compatibility;

public sealed class CraneHoistSystemsCompatibilityTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private RegisterCraneHoistAssetCommandHandler Sut() => new(_repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());

    [Theory]
    [InlineData("OverheadBridgeCrane")]
    [InlineData("overheadbridgecrane")]
    [InlineData("OVERHEADBRIDGECRANE")]
    [InlineData("GantryCrane")]
    [InlineData("JibCrane")]
    [InlineData("MobileCrane")]
    [InlineData("Hoist")]
    public async Task RegisterHandler_AcceptsAllDocumentedAssetTypeSpellings(string assetType)
    {
        var command = new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "COMPAT-1", "Compat Crane", assetType, 1000m, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue($"'{assetType}' should parse as a valid CraneHoistAssetType (case-insensitive)");
    }

    [Fact]
    public void AllCommands_ExposeStableApiVersionTokenForClientNegotiation()
    {
        var register = new RegisterCraneHoistAssetCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "t", "t", "Hoist", 1m, Guid.NewGuid());
        var lift = new RecordLiftEventCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m, Guid.NewGuid());
        var overload = new RecordOverloadEventCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m, Guid.NewGuid());
        var schedule = new ScheduleCraneInspectionCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, Guid.NewGuid());
        var complete = new CompleteCraneInspectionCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, Guid.NewGuid());
        var qualify = new QualifyOperatorCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid());
        var suspend = new SuspendOperatorQualificationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "r", Guid.NewGuid());
        var rigging = new RecordRiggingInspectionCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), false, null, Guid.NewGuid());

        register.ApiVersionToken.Should().Be("v1");
        lift.ApiVersionToken.Should().Be("v1");
        overload.ApiVersionToken.Should().Be("v1");
        schedule.ApiVersionToken.Should().Be("v1");
        complete.ApiVersionToken.Should().Be("v1");
        qualify.ApiVersionToken.Should().Be("v1");
        suspend.ApiVersionToken.Should().Be("v1");
        rigging.ApiVersionToken.Should().Be("v1");
    }

    [Fact]
    public void CraneHoistAssetType_IsAFlagsEnum_SupportingMultiClassAssetRecords()
    {
        // Interop with external asset registries that may tag equipment with more than one class
        // (e.g. a convertible jib/gantry rig) depends on CraneHoistAssetType remaining [Flags].
        typeof(CraneHoistAssetType).IsDefined(typeof(FlagsAttribute), inherit: false).Should().BeTrue();

        var combined = CraneHoistAssetType.OverheadBridgeCrane | CraneHoistAssetType.GantryCrane;
        combined.HasFlag(CraneHoistAssetType.OverheadBridgeCrane).Should().BeTrue();
        combined.HasFlag(CraneHoistAssetType.GantryCrane).Should().BeTrue();
    }
}
