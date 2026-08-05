// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           CredentialCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for IssueCredentialCommandHandler and RevokeCredentialCommandHandler
//                  against a mocked ICredentialRepository. Credential/badge PII fields flow
//                  through the handler but are never asserted in log output here (handler emits
//                  no PII to ILogger by design -- see IssueCredentialCommandHandler remarks).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using CredentialAggregate = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class CredentialCommandHandlerTests : UnitTestBase
{
    private readonly ICredentialRepository _repository = Substitute.For<ICredentialRepository>();

    private IssueCredentialCommandHandler IssueSut() => new(_repository, NullLogger<IssueCredentialCommandHandler>());
    private RevokeCredentialCommandHandler RevokeSut() => new(_repository, NullLogger<RevokeCredentialCommandHandler>());

    private static IssueCredentialCommand ValidIssueCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        BadgeNumber: "BADGE-777",
        HolderUserId: Guid.NewGuid(),
        HolderType: "Employee",
        AccessLevel: "General",
        ExpiresAtUtc: null,
        PerformedByUserId: TestUserId.Value);

    // ---- Issue -------------------------------------------------------------------

    [Fact]
    public async Task Issue_WithValidCommand_ShouldPersistAggregate()
    {
        await IssueSut().Handle(ValidIssueCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<CredentialAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Issue_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await IssueSut().Handle(ValidIssueCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Issue_WithValidCommand_ShouldReturnIssuedStatus()
    {
        var result = await IssueSut().Handle(ValidIssueCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Issued");
    }

    [Fact]
    public async Task Issue_WithUnknownHolderType_ShouldDefaultToEmployee()
    {
        var command = ValidIssueCommand() with { HolderType = "NotARealHolderType" };

        CredentialAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<CredentialAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await IssueSut().Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.HolderType.Should().Be(PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums.CredentialHolderType.Employee);
    }

    [Fact]
    public async Task Issue_WithExpiresAtUtc_ShouldPropagateExpiryToAggregate()
    {
        var expiry = FixedUtcNow.AddYears(2);
        var command = ValidIssueCommand() with { ExpiresAtUtc = expiry };

        CredentialAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<CredentialAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await IssueSut().Handle(command, CancellationToken.None);

        captured!.ExpiresAtUtc!.Value.Value.Should().Be(expiry);
    }

    [Fact]
    public async Task Issue_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<CredentialAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await IssueSut().Handle(ValidIssueCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.Credential.IssueFailed");
    }

    // ---- Revoke ------------------------------------------------------------------

    private static RevokeCredentialCommand RevokeCommand(Guid credentialId, string reason = "Lost badge") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        CredentialId: credentialId,
        Reason: reason,
        PerformedByUserId: TestUserId.Value);

    private static CredentialAggregate IssuedAggregate() =>
        CredentialAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, badgeNumber: "BADGE-300", holderUserId: Guid.NewGuid());

    [Fact]
    public async Task Revoke_WhenFound_ShouldTransitionAggregateAndUpdate()
    {
        var aggregate = IssuedAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await RevokeSut().Handle(RevokeCommand(aggregate.CredentialId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Revoked");
        await _repository.Received(1).UpdateAsync(aggregate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_ShouldPersistProvidedReasonOnAggregate()
    {
        var aggregate = IssuedAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        await RevokeSut().Handle(RevokeCommand(aggregate.CredentialId.Value, "Employee terminated"), CancellationToken.None);

        aggregate.RevocationReason.Should().Be("Employee terminated");
    }

    [Fact]
    public async Task Revoke_WhenNotFound_ShouldReturnFailureWithNotFoundErrorCode()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId>(), Arg.Any<CancellationToken>())
            .Returns((CredentialAggregate?)null);

        var result = await RevokeSut().Handle(RevokeCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.Credential.NotFound");
    }

    [Fact]
    public async Task Revoke_WhenAlreadyRevoked_ShouldReturnFailureResult()
    {
        var aggregate = IssuedAggregate();
        aggregate.Revoke("First revocation", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await RevokeSut().Handle(RevokeCommand(aggregate.CredentialId.Value, "Second attempt"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.Credential.RevokeFailed");
    }
}
