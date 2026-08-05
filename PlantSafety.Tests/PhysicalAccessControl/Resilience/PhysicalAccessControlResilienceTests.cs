// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Resilience.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           PhysicalAccessControlResilienceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Resilience-focused unit tests: proves every PhysicalAccessControl command
//                  handler degrades gracefully (returns a Result.Failure with a stable error
//                  code) rather than letting a repository-layer fault (transient SQL timeout,
//                  connection reset, generic exception) propagate as an unhandled exception --
//                  the "catch, log, return Result.Failure" contract every handler in this module
//                  implements. Also covers repeated/duplicate-call idempotency at the aggregate
//                  guard level (a second identical transition attempt is rejected, not silently
//                  double-applied). Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;
using AccessAnomalyAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly;
using AccessEventAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent;
using AccessPointAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint;
using CredentialAggregate = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential;
using RestrictedZonePolicyAggregate = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy;
using SafetyZoneAggregate = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone;

namespace PlantSafety.Tests.PhysicalAccessControl.Resilience;

/// <summary>
/// Simulates transient infrastructure faults (timeouts, connection resets) at the repository
/// boundary for every PhysicalAccessControl command handler and asserts graceful degradation
/// (a well-formed <c>Result.Failure</c>, never an unhandled exception escaping the handler).
/// </summary>
public sealed class PhysicalAccessControlResilienceTests : UnitTestBase
{
    private static readonly TimeoutException SimulatedTransientFault = new("Simulated SQL command timeout.");

    // ---- AccessPoint -----------------------------------------------------------------

    [Fact]
    public async Task RegisterAccessPoint_OnTransientRepositoryFault_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<IAccessPointRepository>();
        repository.AddAsync(Arg.Any<AccessPointAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw SimulatedTransientFault);

        var handler = new RegisterAccessPointCommandHandler(repository, NullLogger<RegisterAccessPointCommandHandler>());
        var command = new RegisterAccessPointCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "AP-R1", "Resilience test", "BadgeReader", null, false, TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- Credential --------------------------------------------------------------------

    [Fact]
    public async Task IssueCredential_OnTransientRepositoryFault_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<ICredentialRepository>();
        repository.AddAsync(Arg.Any<CredentialAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw SimulatedTransientFault);

        var handler = new IssueCredentialCommandHandler(repository, NullLogger<IssueCredentialCommandHandler>());
        var command = new IssueCredentialCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "BADGE-R1", Guid.NewGuid(), "Employee", "General", null, TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- AccessEvent -------------------------------------------------------------------

    [Fact]
    public async Task IngestAccessEvent_OnTransientRepositoryFault_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<IAccessEventRepository>();
        repository.AddAsync(Arg.Any<AccessEventAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw SimulatedTransientFault);

        var handler = new IngestAccessEventCommandHandler(repository, NullLogger<IngestAccessEventCommandHandler>());
        var command = new IngestAccessEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, Guid.NewGuid(), null, null, "Granted", "In", FixedUtcNow, TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- SafetyZone --------------------------------------------------------------------

    [Fact]
    public async Task RegisterSafetyZone_OnTransientRepositoryFault_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<ISafetyZoneRepository>();
        repository.AddAsync(Arg.Any<SafetyZoneAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw SimulatedTransientFault);

        var handler = new RegisterSafetyZoneCommandHandler(repository, NullLogger<RegisterSafetyZoneCommandHandler>());
        var command = new RegisterSafetyZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "ZONE-R1", "Resilience Zone", "Low", 5, TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- AccessAnomaly -------------------------------------------------------------------

    [Fact]
    public async Task DetectAccessAnomaly_OnTransientRepositoryFault_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<IAccessAnomalyRepository>();
        repository.AddAsync(Arg.Any<AccessAnomalyAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw SimulatedTransientFault);

        var handler = new DetectAccessAnomalyCommandHandler(repository, NullLogger<DetectAccessAnomalyCommandHandler>());
        var command = new DetectAccessAnomalyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, Guid.NewGuid(), Guid.NewGuid(), null, null, "UnauthorizedAccess", 30m, TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- RestrictedZonePolicy ------------------------------------------------------------

    [Fact]
    public async Task DefineRestrictedZonePolicy_OnTransientRepositoryFault_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<IRestrictedZonePolicyRepository>();
        repository.AddAsync(Arg.Any<RestrictedZonePolicyAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw SimulatedTransientFault);

        var handler = new DefineRestrictedZonePolicyCommandHandler(repository, NullLogger<DefineRestrictedZonePolicyCommandHandler>());
        var command = new DefineRestrictedZonePolicyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, Guid.NewGuid(), "Resilience Policy", "General", false, TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- GetByIdAsync failures on transition handlers also degrade gracefully ----------

    [Fact]
    public async Task RevokeCredential_WhenGetByIdThrows_ShouldReturnFailureNotThrow()
    {
        var repository = Substitute.For<ICredentialRepository>();
        repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId>(), Arg.Any<CancellationToken>())
            .Returns<Task<CredentialAggregate?>>(_ => throw SimulatedTransientFault);

        var handler = new RevokeCredentialCommandHandler(repository, NullLogger<RevokeCredentialCommandHandler>());
        var command = new RevokeCredentialCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "Lost", TestUserId.Value);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    // ---- Idempotency guard: repeated lifecycle transitions are rejected, not silently
    //      double-applied (prevents duplicate-delivery / retry-storm side effects) --------

    [Fact]
    public async Task AcknowledgeAccessAnomaly_CalledTwiceInARowForSameAggregate_SecondCallShouldFail()
    {
        var repository = Substitute.For<IAccessAnomalyRepository>();
        var aggregate = AccessAnomalyAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, accessPointId: Guid.NewGuid(), severityScore: 20m);
        repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids.AccessAnomalyId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var handler = new AcknowledgeAccessAnomalyCommandHandler(repository, NullLogger<AcknowledgeAccessAnomalyCommandHandler>());
        var command = new AcknowledgeAccessAnomalyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            aggregate.AccessAnomalyId.Value, Guid.NewGuid(), TestUserId.Value);

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command with { CommandId = Guid.NewGuid() }, CancellationToken.None);

        first.IsSuccess.Should().BeTrue("the first acknowledge should succeed against a freshly-detected anomaly");
        second.IsFailure.Should().BeTrue("a retried/duplicate acknowledge against the now-Acknowledged aggregate must be rejected, not silently re-applied");
    }
}
