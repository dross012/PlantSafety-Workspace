// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           AllowAllTestPermissionService.cs
//  Created:        2026-08-06
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Test-only IPermissionService replacing the real, claims-backed
//                  PermissionService in the integration test host. These integration tests
//                  ("...ShouldRoundTripThroughRealStoredProcedures", "...ThroughRealHandler")
//                  exist to verify MediatR-handler-to-SQL round-trips, not fine-grained
//                  permission-string enforcement -- that's PlantSafety.SecurityTests' job.
//                  Enumerating every IAuthorizedRequest.RequiredPermission string across the
//                  whole codebase just to seed matching claims on TestAuthHandler's principal
//                  would be fragile busywork unrelated to what these tests actually assert.
//                  Mirrors the existing IDistributedCache override in InitTestHost.cs --
//                  same "swap the out-of-process/cross-cutting dependency for a test double,
//                  don't fight it" pattern, already established in this file before this change.
//                  Still verifies identity (userId/tenantId match TestIds), just skips the
//                  specific permission-string match. Additive-only evolution.
// ============================================================================

using PlantSafety.Application.Abstractions.Security;
using PlantSafety.IntegrationTests.Common.TestConstants;

namespace PlantSafety.IntegrationTests.Setup;

/// <summary>Grants every permission to the fixed <see cref="TestIds"/> test identity; denies anyone else.</summary>
public sealed class AllowAllTestPermissionService : IPermissionService
{
    /// <inheritdoc/>
    public Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission, CancellationToken cancellationToken = default) =>
        Task.FromResult(Matches(userId, tenantId));

    /// <inheritdoc/>
    public Task<bool> HasAnyPermissionAsync(Guid userId, Guid tenantId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default) =>
        Task.FromResult(Matches(userId, tenantId));

    /// <inheritdoc/>
    public Task<bool> HasAllPermissionsAsync(Guid userId, Guid tenantId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default) =>
        Task.FromResult(Matches(userId, tenantId));

    private static bool Matches(Guid userId, Guid tenantId) =>
        userId == TestIds.ActorId && tenantId == TestIds.TenantId;
}
