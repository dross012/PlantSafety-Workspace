// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Api.Middleware
//  Layer:          Tests (Unit)
//  File:           TenantResolutionMiddlewareTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 alignment governance fix)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for TenantResolutionMiddleware's strict impersonation-override
//                  validation: the JWT tenantId claim is always the default, and an
//                  X-Tenant-ID/X-Impersonation-Session-Id header pair only overrides it when
//                  every check (Platform.Admin role, resolvable caller UserId, a real active
//                  session, session ownership, session-tenant binding) passes. Any single failed
//                  check must fall back to the JWT tenant unchanged -- no header-only tenant
//                  switching under any circumstance. Isolated, deterministic, no I/O beyond the
//                  mocked ICacheService.
// ============================================================================
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PlantSafety.Api.Constants;
using PlantSafety.Api.CrossDomain;
using PlantSafety.Api.Middleware;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Support.Impersonation.Models;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Tests.Api.Middleware;

/// <summary>Unit tests for <see cref="TenantResolutionMiddleware"/>.</summary>
public sealed class TenantResolutionMiddlewareTests : UnitTestBase
{
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private static readonly Guid JwtTenantId = Guid.NewGuid();
    private static readonly Guid AdminUserId = Guid.NewGuid();
    private static readonly Guid ImpersonatedTenantId = Guid.NewGuid();

    private TenantResolutionMiddleware Sut(RequestDelegate next) => new(next, NullLogger<TenantResolutionMiddleware>());

    private static DefaultHttpContext BuildAuthenticatedContext(bool isPlatformAdmin, Guid? callerUserId = null)
    {
        var claims = new List<Claim>
        {
            new("tenantId", JwtTenantId.ToString()),
            new("sub", (callerUserId ?? AdminUserId).ToString()),
        };

        if (isPlatformAdmin)
        {
            claims.Add(new Claim("role", SafetyRoles.PlatformAdmin));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        context.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey] = Guid.NewGuid().ToString();
        return context;
    }

    // Session activity is judged by the middleware against real wall-clock time
    // (ImpersonationSessionDto.IsActive(DateTime.UtcNow)), not the suite's fixed test timestamp --
    // so these fixtures must be built relative to DateTime.UtcNow, not FixedUtcNow.
    private static ImpersonationSessionDto ActiveSession(Guid tenantId, Guid adminUserId, Guid sessionId) => new(
        SessionId: sessionId,
        TenantId: tenantId,
        AdminUserId: adminUserId,
        Reason: "test",
        StartedAtUtc: DateTime.UtcNow,
        ExpiresAtUtc: DateTime.UtcNow.AddMinutes(30),
        EndedAtUtc: null,
        CorrelationId: Guid.NewGuid());

    [Fact]
    public async Task InvokeAsync_NoImpersonationHeaders_ResolvesJwtTenantUnchanged()
    {
        var context = BuildAuthenticatedContext(isPlatformAdmin: false);

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
        await _cacheService.DidNotReceiveWithAnyArgs().GetAsync<ImpersonationSessionDto>(default!, default);
    }

    [Fact]
    public async Task InvokeAsync_ValidImpersonation_OverridesToImpersonatedTenant()
    {
        var sessionId = Guid.NewGuid();
        var context = BuildAuthenticatedContext(isPlatformAdmin: true);
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId] = sessionId.ToString();

        _cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), Arg.Any<CancellationToken>())
            .Returns(ActiveSession(ImpersonatedTenantId, AdminUserId, sessionId));

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(ImpersonatedTenantId);
    }

    [Fact]
    public async Task InvokeAsync_CallerNotPlatformAdmin_IgnoresHeadersFallsBackToJwtTenant()
    {
        var sessionId = Guid.NewGuid();
        var context = BuildAuthenticatedContext(isPlatformAdmin: false);
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId] = sessionId.ToString();

        _cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), Arg.Any<CancellationToken>())
            .Returns(ActiveSession(ImpersonatedTenantId, AdminUserId, sessionId));

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
    }

    [Fact]
    public async Task InvokeAsync_SessionNotFound_IgnoresHeadersFallsBackToJwtTenant()
    {
        var sessionId = Guid.NewGuid();
        var context = BuildAuthenticatedContext(isPlatformAdmin: true);
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId] = sessionId.ToString();

        _cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), Arg.Any<CancellationToken>())
            .Returns((ImpersonationSessionDto?)null);

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
    }

    [Fact]
    public async Task InvokeAsync_ExpiredSession_IgnoresHeadersFallsBackToJwtTenant()
    {
        var sessionId = Guid.NewGuid();
        var context = BuildAuthenticatedContext(isPlatformAdmin: true);
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId] = sessionId.ToString();

        var expired = new ImpersonationSessionDto(
            SessionId: sessionId, TenantId: ImpersonatedTenantId, AdminUserId: AdminUserId, Reason: "test",
            StartedAtUtc: DateTime.UtcNow.AddHours(-2), ExpiresAtUtc: DateTime.UtcNow.AddHours(-1), EndedAtUtc: null,
            CorrelationId: Guid.NewGuid());

        _cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), Arg.Any<CancellationToken>())
            .Returns(expired);

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
    }

    [Fact]
    public async Task InvokeAsync_SessionBelongsToDifferentAdmin_IgnoresHeadersFallsBackToJwtTenant()
    {
        var sessionId = Guid.NewGuid();
        var otherAdminId = Guid.NewGuid();
        var context = BuildAuthenticatedContext(isPlatformAdmin: true, callerUserId: AdminUserId);
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId] = sessionId.ToString();

        // A live, otherwise-valid session -- but it belongs to a DIFFERENT admin than the caller.
        _cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), Arg.Any<CancellationToken>())
            .Returns(ActiveSession(ImpersonatedTenantId, otherAdminId, sessionId));

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
    }

    [Fact]
    public async Task InvokeAsync_SessionBoundToDifferentTenantThanHeaderClaims_IgnoresHeadersFallsBackToJwtTenant()
    {
        var sessionId = Guid.NewGuid();
        var actualSessionTenantId = Guid.NewGuid();
        var context = BuildAuthenticatedContext(isPlatformAdmin: true);
        // Header claims ImpersonatedTenantId, but the real session record is bound to a different tenant.
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId] = sessionId.ToString();

        _cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), Arg.Any<CancellationToken>())
            .Returns(ActiveSession(actualSessionTenantId, AdminUserId, sessionId));

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
    }

    [Fact]
    public async Task InvokeAsync_OnlyTenantIdHeaderPresentNoSessionIdHeader_IgnoresHeaderFallsBackToJwtTenant()
    {
        var context = BuildAuthenticatedContext(isPlatformAdmin: true);
        context.Request.Headers[ApiHeaderConstants.TenantId] = ImpersonatedTenantId.ToString();
        // No X-Impersonation-Session-Id header at all -- the historical, broken "header-only" shape.

        await Sut(_ => Task.CompletedTask).InvokeAsync(context, _cacheService);

        context.Items[TenantContextAccessor.TenantIdKey].Should().Be(JwtTenantId);
        await _cacheService.DidNotReceiveWithAnyArgs().GetAsync<ImpersonationSessionDto>(default!, default);
    }
}
