// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Tenancy
//  Layer:          IntegrationTests
//  File:           TenantHttpContextTestHelper.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.1/§5.4)
//  SchemaVersion:  2026.1
//  Description:    Shared helper for tests in this folder that need to simulate a
//                  server-resolved tenant context (as production's TenantResolutionMiddleware
//                  populates from JWT claims) before sending a real request through the real,
//                  DI-registered MediatR pipeline (TenantIsolationBehavior reads
//                  IOperationContext.TenantId, which OperationContextAccessor resolves from
//                  HttpContext.Items). Deterministic, parallel-safe, additive-only.
// ============================================================================
using Microsoft.AspNetCore.Http;

namespace PlantSafety.IntegrationTests.Tenancy;

/// <summary>
/// Shared helper constructing a DI scope whose <see cref="IHttpContextAccessor"/> carries a real
/// <see cref="DefaultHttpContext"/> stamped with a server-resolved tenant id, mirroring the
/// <c>Items</c> key convention <c>PlantSafety.Api.CrossDomain.TenantContextAccessor</c> and
/// <c>PlantSafety.Api.Context.OperationContextAccessor</c> both read from.
/// </summary>
internal static class TenantHttpContextTestHelper
{
    /// <summary>The <c>HttpContext.Items</c> key production's TenantResolutionMiddleware populates from JWT claims.</summary>
    public const string TenantIdItemsKey = "PlantSafety:TenantId";

    /// <summary>
    /// Creates a DI scope from <paramref name="factory"/> whose <see cref="IHttpContextAccessor"/>
    /// carries a real <see cref="DefaultHttpContext"/> stamped with
    /// <paramref name="serverResolvedTenantId"/>, so <c>IOperationContext.TenantId</c> resolves to
    /// it for any request sent via <c>ISender</c> from the returned scope.
    /// </summary>
    public static IServiceScope CreateScopeWithResolvedTenant(IntegrationTestWebAppFactory factory, Guid serverResolvedTenantId)
    {
        var scope = factory.Services.CreateScope();

        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider,
        };
        httpContextAccessor.HttpContext.Items[TenantIdItemsKey] = serverResolvedTenantId;

        return scope;
    }
}
