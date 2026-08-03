// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           SetTenantFeatureFlagOverrideCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SetTenantFeatureFlagOverrideCommandHandler.
// ============================================================================
using System;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Operations.Configuration.Commands;
using PlantSafety.Application.Operations.Configuration.Handlers;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Configuration.Handlers;

public sealed class SetTenantFeatureFlagOverrideCommandHandlerTests : UnitTestBase
{
    private readonly ITenantFeatureFlagOverrideRepository _repository = Substitute.For<ITenantFeatureFlagOverrideRepository>();
    private readonly ITenantReadRepository _tenantReadRepository = Substitute.For<ITenantReadRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    // Phase 7 §3.4 License Enforcement (Billing agent, 2026-08-03): SetTenantFeatureFlagOverrideCommandHandler
    // now also depends on ITenantReadRepository to enforce plan-based module-access restrictions
    // when ENABLING an override. Returning null (the NSubstitute default) here means "tenant not
    // found", which the handler treats as "skip the plan check" -- preserving every pre-existing
    // test's behavior unchanged.
    private SetTenantFeatureFlagOverrideCommandHandler Sut() =>
        new(_repository, _tenantReadRepository, _currentUserService, NullLogger<SetTenantFeatureFlagOverrideCommandHandler>());

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldPassCurrentUserIdAsUpdatedBy()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        var tenantId = Guid.NewGuid();
        var expected = new TenantFeatureFlagOverrideDto(tenantId, "Intelligence.NewDashboard", true, 40, FixedUtcNow, TestUserId.Value.ToString());
        _repository.SetAsync(tenantId, "Intelligence.NewDashboard", true, 40, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await Sut().Handle(new SetTenantFeatureFlagOverrideCommand(tenantId, "Intelligence.NewDashboard", true, 40), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
        await _repository.Received(1).SetAsync(tenantId, "Intelligence.NewDashboard", true, 40, TestUserId.Value.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldFallBackToSystemIdentity()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        var tenantId = Guid.NewGuid();

        _repository.SetAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TenantFeatureFlagOverrideDto(tenantId, "Feature.X", false, null, FixedUtcNow, "System"));

        await Sut().Handle(new SetTenantFeatureFlagOverrideCommand(tenantId, "Feature.X", false, null), CancellationToken.None);

        await _repository.Received(1).SetAsync(tenantId, "Feature.X", false, null, "System", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);
        var tenantId = Guid.NewGuid();

        _repository.SetAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new SetTenantFeatureFlagOverrideCommand(tenantId, "Feature.X", true, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.TenantOverrideFailed", result.Error.ErrorCode);
    }
}
