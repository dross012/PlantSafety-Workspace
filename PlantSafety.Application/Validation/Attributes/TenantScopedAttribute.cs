// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Marker attribute to document that a query or command property is tenant-scoped.
//   Documents multi-tenancy contract for developers and API tooling.
// SchemaVersion: 1

namespace PlantSafety.Application.Validation.Attributes;

/// <summary>
/// Documents that a property participates in multi-tenancy scoping.
/// Properties marked with this attribute are validated to be non-empty
/// and are used to restrict query/command results to a single tenant.
/// Cross-tenant access is forbidden — the Infrastructure layer enforces this via stored procedures.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class TenantScopedAttribute : Attribute;
