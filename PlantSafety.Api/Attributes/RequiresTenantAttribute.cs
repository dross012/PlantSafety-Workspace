// ============================================================================
// File: RequiresTenantAttribute.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Marker attribute indicating that the decorated action or controller
//              requires a resolved tenant context. Used by IdempotencyFilter and
//              AuditLogFilter to determine whether tenant-scoped processing applies.
// ============================================================================
//
// Onboarding Notes:
// - Apply to all controller actions that operate on tenant data. Every module
//   controller should apply this at the class level:
//     [RequiresTenant]
//     public sealed class AuditFindingsV1Controller : BaseApiController { ... }
// - Do NOT apply to health-check endpoints or anonymous service endpoints.
// - IdempotencyFilter reads this attribute via IFilterMetadata; without it, the
//   idempotency cache key cannot be safely tenant-scoped and the filter is skipped.
// - This attribute does not itself enforce authentication — [Authorize] must also
//   be present. [RequiresTenant] is a metadata/routing concern, not a security gate.
//
// Rationale:
// - Provides a declarative marker so filters can distinguish tenant-scoped actions
//   from platform-level actions (health, metrics, Swagger) without inspecting
//   route templates or claim presence at runtime.
//
// Future Extension Seams:
// - Add AllowGlobalTenant (bool) property to indicate that tenant-global requests
//   (no siteId) are permitted for the decorated endpoint, enabling filters to
//   enforce site-scoped access where required.
//
// ============================================================================

namespace PlantSafety.Api.Attributes;

/// <summary>
/// Marker attribute indicating that the decorated controller or action requires
/// a resolved tenant context (populated by <see cref="Middleware.TenantResolutionMiddleware"/>).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequiresTenantAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether requests without a site ID are permitted.
    /// When <c>false</c> (default), only site-scoped requests are served.
    /// When <c>true</c>, tenant-global requests (no siteId claim) are also permitted.
    /// </summary>
    public bool AllowGlobalTenant { get; set; }
}
