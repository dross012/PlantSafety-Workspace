// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Typed API client for the Admin Portal's User Management page. Wraps HTTP
//   communication for PlantSafety.Api.Features.EnterpriseFabric.Users.Controllers.V1
//   .UsersV1Controller (list/create). No deactivate/reactivate endpoint exists on the real
//   backend today -- IUserRepository has no corresponding repository method or stored
//   procedure (only usp_User_Create/GetById/List), so this client intentionally does not
//   expose a Deactivate/Reactivate method. See UserManagementPage.razor for the UI-facing
//   note.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>Typed API client for the Admin Portal's User Management page.</summary>
public sealed class UserManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="UserManagementApiClient"/>.</summary>
    public UserManagementApiClient(HttpClient http, ILogger<UserManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Lists users for the current tenant, paged and optionally filtered.</summary>
    public Task<ApiResponseEnvelope<PagedUsersDto>> ListAsync(
        string? role = null,
        Guid? siteId = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = $"pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(role)) query += $"&role={Uri.EscapeDataString(role)}";
        if (siteId is not null) query += $"&siteId={siteId}";
        if (isActive is not null) query += $"&isActive={isActive}";

        return GetAsync<PagedUsersDto>($"v1/features/enterprise-fabric/users?{query}", cancellationToken);
    }

    /// <summary>Creates a new user account within the tenant. Throws no client-side exception when
    /// the tenant is at its subscription plan's user cap -- the resulting 409 Conflict problem is
    /// surfaced through the returned envelope's <c>Problem.Detail</c>, populated from the real
    /// <c>UserLimitExceededException</c> message.</summary>
    public Task<ApiResponseEnvelope<CreateUserResponseDto>> CreateAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateUserRequest, CreateUserResponseDto>("v1/features/enterprise-fabric/users", request, cancellationToken);
}
