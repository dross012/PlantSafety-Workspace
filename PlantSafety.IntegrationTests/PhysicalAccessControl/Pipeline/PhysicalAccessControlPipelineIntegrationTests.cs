// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           PhysicalAccessControlPipelineIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Functional/End-to-End integration coverage sending real PhysicalAccessControl
//                  commands and queries through the real, DI-resolved MediatR pipeline
//                  (ValidationBehavior -> handler -> real Dapper repository -> real SQL Server),
//                  using the same TenantHttpContextTestHelper other module suites use to simulate
//                  a server-resolved tenant without a full JWT identity provider (see
//                  PlantSafety.IntegrationTests.Tenancy.TenantHttpContextTestHelper). Also proves
//                  ValidationBehavior rejects a malformed command before it ever reaches the
//                  handler/repository (a genuine end-to-end validation-pipeline test, not a bare
//                  FluentValidation.TestHelper unit test).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Queries;
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.IntegrationTests.Tenancy;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Pipeline;

[Collection(nameof(IntegrationTestCollection))]
public sealed class PhysicalAccessControlPipelineIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact(DisplayName = "PLNT-FUNC-PAC-001: RegisterAccessPointCommand sent through the real MediatR pipeline persists via the real Dapper repository and is retrievable via the real read-model query")]
    public async Task RegisterAccessPoint_ThenGetById_ShouldRoundTripThroughRealPipelineAndDatabase()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new RegisterAccessPointCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId,
            null, $"AP-PIPE-{Guid.NewGuid():N}"[..12], "Pipeline integration test", "VehicleGate", null, true, ActorId);

        var commandResult = await sender.Send(command, CancellationToken.None);

        commandResult.IsSuccess.Should().BeTrue();
        commandResult.Value.Status.Should().Be("Registered");

        var query = new GetAccessPointByIdQuery(TenantId, commandResult.Value.AccessPointId);
        var queryResult = await sender.Send(query, CancellationToken.None);

        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.AccessPoint.Should().NotBeNull();
        queryResult.Value.AccessPoint!.AccessPointCode.Should().Be(command.AccessPointCode);
        queryResult.Value.AccessPoint.IsPerimeter.Should().BeTrue();
    }

    [Fact(DisplayName = "PLNT-FUNC-PAC-002: DecommissionAccessPointCommand for an unknown Id returns a NotFound Result through the real pipeline, not an exception")]
    public async Task DecommissionAccessPoint_ForUnknownId_ShouldReturnNotFoundResultThroughRealPipeline()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new DecommissionAccessPointCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, Guid.NewGuid(), ActorId);

        var result = await sender.Send(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessPoint.NotFound");
    }

    [Fact(DisplayName = "PLNT-SEC-PAC-003: ValidationBehavior rejects a malformed IssueCredentialCommand (empty BadgeNumber) before it reaches the handler")]
    public async Task IssueCredential_WithEmptyBadgeNumber_ShouldBeRejectedByValidationBehaviorBeforeReachingHandler()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new IssueCredentialCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId,
            null, string.Empty, Guid.NewGuid(), "Employee", "General", null, ActorId);

        var act = async () => await sender.Send(command, CancellationToken.None);

        // ValidationBehavior throws FluentValidation.ValidationException for malformed requests --
        // the pipeline rejects them before the handler (and therefore before any repository/
        // database call) ever runs.
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "PLNT-FUNC-PAC-004: RegisterSafetyZone -> RestrictSafetyZone lifecycle transition round-trips through the real pipeline and database")]
    public async Task RegisterSafetyZone_ThenRestrict_ShouldPersistRestrictedStatusThroughRealPipeline()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var registerCommand = new RegisterSafetyZoneCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId,
            null, $"ZN-PIPE-{Guid.NewGuid():N}"[..10], "Pipeline Test Zone", "High", 15, ActorId);

        var registerResult = await sender.Send(registerCommand, CancellationToken.None);
        registerResult.IsSuccess.Should().BeTrue();

        var restrictCommand = new RestrictSafetyZoneCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, registerResult.Value.SafetyZoneId, ActorId);

        var restrictResult = await sender.Send(restrictCommand, CancellationToken.None);

        restrictResult.IsSuccess.Should().BeTrue();
        restrictResult.Value.Status.Should().Be("Restricted");
    }
}
