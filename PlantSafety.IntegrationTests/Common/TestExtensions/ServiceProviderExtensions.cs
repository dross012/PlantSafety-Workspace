// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           ServiceProviderExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Extension methods that resolve scoped services from the test DI container.
//                  Deterministic, parallel-safe, additive-only.
//  UPDATED:        2026-08-06 -- root-cause fix for the systemic tenant-context test-harness
//                  failure (ApplicationAuthorizationException: "The request's TenantId does not
//                  match the authenticated tenant context.", ~125 of ~135 integration test
//                  failures). This is the single choke point every test in this suite that
//                  resolves ISender directly (the majority -- ~50 files call
//                  GetScopedService<ISender>() rather than going through Client/HTTP) passes
//                  through. IOperationContext.TenantId and ICurrentUserService are both backed by
//                  IHttpContextAccessor.HttpContext; a plain scope.ServiceProvider.
//                  GetRequiredService<ISender>() call has no ambient HttpContext at all (there was
//                  never an HTTP request), so both fell back to their "no context" defaults --
//                  Guid.Empty / unauthenticated -- regardless of TestAuthHandler (which only
//                  covers the HTTP/Client path; adding it alone did not fix this). Populating a
//                  fake authenticated HttpContext here, once, fixes every caller of
//                  GetScopedService without touching the ~50 individual test files.
// ============================================================================

using Microsoft.AspNetCore.Http;
using PlantSafety.Api.CrossDomain;
using PlantSafety.IntegrationTests.Common.TestConstants;
using PlantSafety.IntegrationTests.Setup;

namespace PlantSafety.IntegrationTests.Common.TestExtensions;

/// <summary>
/// Extension methods for resolving services from the test <see cref="IServiceProvider"/>
/// within integration tests.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates a new DI scope, establishes a fake authenticated <see cref="HttpContext"/> (fixed
    /// <see cref="TestIds"/> identity, tenant/site already resolved into
    /// <see cref="TenantContextAccessor"/>'s HttpContext.Items keys as TenantResolutionMiddleware
    /// would have done for a real HTTP request), and resolves the requested service.
    /// The caller is responsible for disposing the scope.
    /// </summary>
    public static (IServiceScope Scope, T Service) GetScopedService<T>(
        this IServiceProvider serviceProvider) where T : notnull
    {
        var scope = serviceProvider.CreateScope();

        var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
        if (httpContextAccessor is not null && httpContextAccessor.HttpContext is null)
        {
            var context = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider,
                User = TestPrincipal.Create(),
            };
            context.Items[TenantContextAccessor.TenantIdKey] = TestIds.TenantId;
            context.Items[TenantContextAccessor.SiteIdKey] = TestIds.SiteId;
            httpContextAccessor.HttpContext = context;
        }

        var service = scope.ServiceProvider.GetRequiredService<T>();
        return (scope, service);
    }
}
