# API Documentation Status (Phase 7 §6.4)

## Summary

PlantSafety.Api already serves live, generated OpenAPI/Swagger documentation. This document
records how to access it and does not introduce a parallel documentation system.

## How Swagger/OpenAPI is wired

- `PlantSafety.Api\Extensions\SwaggerExtensions.cs` registers Swashbuckle (`AddSwaggerGen`) and,
  at the bottom of the request pipeline, calls `app.UseSwagger(...)` and `app.UseSwaggerUI(...)`
  (see lines ~122/129 of that file).
- Every controller action across all ~266 `*V1Controller.cs` files (see
  `docs\Phase7\module-docs-index.md` for the full list) is annotated with
  `[ProducesResponseType]` attributes, so response shapes are reflected accurately in the
  generated spec.
- Two `IOperationFilter`s enrich every documented operation:
  - `PlantSafety.Api\OpenApi\CorrelationIdOperationFilter.cs` documents the correlation-id
    request/response header contract.
  - `PlantSafety.Api\OpenApi\TenantIdHeaderFilter.cs` documents the optional `X-Tenant-ID`
    header (see `PlantSafety.Api\Constants\ApiHeaderConstants.cs`), which
    `TenantResolutionMiddleware` honors as an override of the JWT `tenantId` claim. This is the
    same header the Admin Portal's `ImpersonationHeaderHandler`
    (`PlantSafety.UI\Infrastructure\Impersonation\ImpersonationHeaderHandler.cs`) stamps while a
    Support Console impersonation session is active.
- API versioning is handled by `Asp.Versioning` (`[ApiVersion("1.0")]` on every controller,
  routed via `api/v{version:apiVersion}/...`).

## Accessing the live spec

- Run `PlantSafety.Api` locally (or point at any deployed environment) and browse to the
  Swagger UI route configured in `SwaggerExtensions.cs` (the conventional `/swagger` path for
  this ASP.NET Core pipeline).
- The raw OpenAPI JSON/YAML document is served alongside the UI by the same
  `app.UseSwagger(...)` middleware registration.

## New Phase 7 §6 endpoints reflected automatically

No manual documentation step is required for the controllers added in this Phase 7 session —
Swashbuckle reflects them the moment the API process starts, because they use the same
`[ApiController]`/`[ProducesResponseType]`/`[Tags(...)]` conventions as every existing
controller:

- `PlantSafety.Api\Billing\Controllers\V1\BillingV1Controller.cs` (`Tags("Billing")`)
- `PlantSafety.Api\Support\Impersonation\Controllers\V1\ImpersonationV1Controller.cs`
  (`Tags("Support.Impersonation")`)
- `PlantSafety.Api\Auditing\ComplianceMonitoring\Controllers\V1\ComplianceMonitoringV1Controller.cs`
  (`Tags("Auditing.ComplianceMonitoring")`)

## Recommendation

No further documentation-system work is needed for API surface discovery. If a published,
versioned API reference site (outside the live Swagger UI) is later required, the existing
OpenAPI JSON output can be fed into a static-site generator (e.g. Redoc) as a follow-up — that
is a packaging/publishing decision, not a documentation-generation gap.
