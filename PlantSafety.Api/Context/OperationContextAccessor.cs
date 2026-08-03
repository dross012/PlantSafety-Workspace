// ============================================================================
// File: OperationContextAccessor.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Presentation-layer implementation of IOperationContext. Resolves the full
//              correlation lineage envelope from HttpContext.Items (populated by
//              CorrelationIdMiddleware and TenantResolutionMiddleware) and the current
//              System.Diagnostics.Activity for distributed tracing identifiers.
// ============================================================================
//
// Onboarding Notes:
// - Registered as Scoped in DI (one instance per HTTP request), matching the per-request
//   lifetime of IHttpContextAccessor.
// - TraceId/SpanId come from Activity.Current (populated automatically by ASP.NET Core's
//   built-in W3C trace context propagation); a fresh identifier is generated as a fallback
//   for non-HTTP contexts (background jobs, tests) where no Activity is present.
// - CausationId is null for root API-initiated operations, per the Canonical Lineage Rule
//   (§2.17-2.18) — root commands generate a new CausationId only when they themselves cause
//   a child operation, they don't inherit one from nothing.
//
// ============================================================================

using System.Security.Claims;
using PlantSafety.Api.CrossDomain;
using PlantSafety.Application.Abstractions.Context;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Api.Context;

/// <summary>
/// Resolves <see cref="IOperationContext"/> from the current HTTP request's middleware-populated
/// <see cref="HttpContext.Items"/> and the ambient <see cref="System.Diagnostics.Activity"/>.
/// </summary>
public sealed class OperationContextAccessor : IOperationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Guid _requestId = Guid.NewGuid();
    private readonly DateTime _operationStartedAtUtc = DateTime.UtcNow;

    /// <summary>Initialises a new instance of <see cref="OperationContextAccessor"/>.</summary>
    /// <param name="httpContextAccessor">ASP.NET Core HTTP context accessor.</param>
    public OperationContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public Guid CorrelationId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.Items[Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString();
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    /// <inheritdoc/>
    public Guid? CausationId => null;

    /// <inheritdoc/>
    public Guid RequestId => _requestId;

    /// <inheritdoc/>
    public string TraceId =>
        System.Diagnostics.Activity.Current?.TraceId.ToString() ?? _requestId.ToString("N");

    /// <inheritdoc/>
    public string SpanId =>
        System.Diagnostics.Activity.Current?.SpanId.ToString() ?? _requestId.ToString("N")[..16];

    /// <inheritdoc/>
    public string? ParentOperationId => null;

    /// <inheritdoc/>
    public Guid TenantId =>
        _httpContextAccessor.HttpContext?.Items[TenantContextAccessor.TenantIdKey] is Guid g ? g : Guid.Empty;

    /// <inheritdoc/>
    public Guid? SiteId =>
        _httpContextAccessor.HttpContext?.Items[TenantContextAccessor.SiteIdKey] as Guid?;

    /// <inheritdoc/>
    public DateTime OperationStartedAtUtc => _operationStartedAtUtc;

    /// <inheritdoc/>
    public RoleContext RoleContext
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return RoleContext.Empty;
            }

            var roleClaimValues = user.FindAll(ClaimTypes.Role)
                .Concat(user.FindAll("role"))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var highestRole = RoleInheritanceResolver.ResolveHighestRole(roleClaimValues);

            return new RoleContext(roleClaimValues, highestRole);
        }
    }

    /// <inheritdoc/>
    public ComplianceContext ComplianceContext
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var regulatoryScope = user?.FindFirst(ComplianceScopeClaimType)?.Value;

            return string.IsNullOrWhiteSpace(regulatoryScope)
                ? ComplianceContext.None
                : new ComplianceContext(IsRegulatedSession: true, RegulatoryScope: regulatoryScope);
        }
    }

    /// <summary>
    /// Claim type carrying the current session's regulatory scope, when the identity
    /// provider or tenant configuration asserts one (e.g. <c>"OSHA"</c>, <c>"ISO45001"</c>).
    /// Absent by default — most sessions are non-regulated, matching this codebase's
    /// established graceful-degrade convention for optional context.
    /// </summary>
    private const string ComplianceScopeClaimType = "compliance_scope";
}
