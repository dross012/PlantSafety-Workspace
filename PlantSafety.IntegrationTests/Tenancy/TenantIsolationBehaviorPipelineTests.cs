// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Tenancy
//  Layer:          IntegrationTests
//  File:           TenantIsolationBehaviorPipelineTests.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.1)
//  SchemaVersion:  2026.1
//  Description:    Proves TenantIsolationBehavior<,> rejects a mismatched request TenantId
//                  through the REAL, DI-resolved MediatR pipeline (ApplicationServiceRegistration.
//                  AddApplicationServices' actual IPipelineBehavior<,> chain: LoggingBehavior ->
//                  MetricsBehavior -> PerformanceBehavior -> TenantIsolationBehavior ->
//                  AuthorizationBehavior -> CachingBehavior -> ValidationBehavior ->
//                  ConcurrencyBehavior -> WorkflowInstanceLockBehavior -> TransactionBehavior),
//                  not a bare unit test constructing TenantIsolationBehavior in isolation
//                  (PlantSafety.Tests already covers that shape separately).
//
//                  IOperationContext.TenantId (PlantSafety.Api.Context.OperationContextAccessor)
//                  is resolved from the ambient HttpContext.Items[TenantContextAccessor.TenantIdKey]
//                  -- populated in production by TenantResolutionMiddleware from the caller's JWT
//                  claims. This suite simulates exactly that server-resolved state by constructing
//                  a real DefaultHttpContext, stamping the resolved tenant onto it the same way the
//                  real middleware does, and handing it to the real, DI-registered
//                  IHttpContextAccessor before resolving ISender from that same scope and sending a
//                  real MediatR request through the real pipeline -- an honest proxy for "the
//                  server already authenticated/resolved Tenant A, and the request body claims
//                  Tenant B," without needing a full JWT-issuing identity provider inside this test
//                  suite (no such provider is wired into the test host today -- see
//                  IntegrationTestWebAppFactory).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.AccidentManagement.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Tenancy;

/// <summary>
/// Proves <c>TenantIsolationBehavior&lt;,&gt;</c> rejects a cross-tenant request end-to-end
/// through the real, DI-registered MediatR pipeline (not a unit test of the behavior alone).
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class TenantIsolationBehaviorPipelineTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private static readonly Guid ServerResolvedTenant = new("cccccccc-3333-3333-3333-333333333333");
    private static readonly Guid RequestClaimedTenant = new("dddddddd-4444-4444-4444-444444444444");

    [Fact(DisplayName = "PLNT-SEC-TENANT-PIPELINE-001: A request whose TenantId does not match the server-resolved tenant is rejected by the real MediatR pipeline")]
    public async Task Send_RequestWithMismatchedTenantId_ThrowsApplicationAuthorizationException_ThroughRealPipeline()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, ServerResolvedTenant);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // The server has resolved (authenticated) the caller as ServerResolvedTenant, but the
        // request body/query string claims a completely different tenant -- exactly the class of
        // attack TenantIsolationBehavior exists to close (a malicious/misconfigured client passing
        // an arbitrary TenantId).
        var query = new GetAccidentByIdQuery(RequestClaimedTenant, Guid.NewGuid());

        var act = async () => await sender.Send(query, CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<ApplicationAuthorizationException>(
            "TenantIsolationBehavior must reject any request whose bound TenantId does not match " +
            "IOperationContext.TenantId, before the handler (and therefore before any repository call) ever runs");

        thrown.Which.Error.ErrorCode.Should().Be(ErrorCodes.TenantMismatch);
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-PIPELINE-002: A request whose TenantId matches the server-resolved tenant passes through the real pipeline (no false positive)")]
    public async Task Send_RequestWithMatchingTenantId_DoesNotThrowForTenantMismatch()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, ServerResolvedTenant);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // Same tenant on both sides -- TenantIsolationBehavior must be transparent here. The query
        // still resolves to a NotFound Result (no such Accident exists), proving the request
        // reached the real handler rather than being rejected -- the negative-case companion to
        // PLNT-SEC-TENANT-PIPELINE-001, guarding against a security fix that is simply "always
        // throw" rather than a genuine tenant comparison.
        var query = new GetAccidentByIdQuery(ServerResolvedTenant, Guid.NewGuid());

        var result = await sender.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse("no Accident exists with this Id, so the real handler must run and return a NotFound Result");
        result.Error.ErrorCode.Should().Be("AccidentManagement.Accident.NotFound",
            "reaching this specific error code proves the request passed through TenantIsolationBehavior into the real GetAccidentByIdQueryHandler, " +
            "rather than being rejected earlier in the pipeline");
    }
}
