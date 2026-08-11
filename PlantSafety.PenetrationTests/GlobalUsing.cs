// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PenetrationTests
//  Layer:          Tests (Penetration)
//  Description:    Global usings for the penetration suite (PLNT-PEN-*).
//
//                  SCOPE. This suite attacks THIS codebase's own security
//                  controls, in-process, with no network and no live host. It
//                  drives the same public entry points an attacker's input
//                  would reach and asserts the control refuses. Nothing here
//                  scans, connects to, or exercises a deployed environment.
//
//                  POSTURE. Where PlantSafety.SecurityTests asserts that a
//                  control behaves correctly on well-formed input, these tests
//                  assume the caller is hostile: the tenant id is forged, the
//                  procedure name is a SQL payload, the diagnostics endpoint is
//                  being read to harvest secrets. A test passes only when the
//                  attack is REFUSED -- so each one must be able to fail if the
//                  control is removed, which is why they assert on the specific
//                  rejection rather than merely that something was thrown.
//
//                  BOUNDARIES. Three surfaces are covered, chosen because each
//                  is a single seam that many callers funnel through:
//                    * Tenancy   -- cross-tenant escalation via a client-
//                                   supplied TenantId (~148 request DTOs).
//                    * Injection -- SQL text smuggled through the one string
//                                   the SQL gateway accepts: the procedure name.
//                    * Disclosure-- secret values reaching logs or diagnostics.
//
//                  Live-host concerns (TLS, rate limiting, authentication
//                  against a running endpoint, dependency CVEs) are out of
//                  scope here and belong to an authorised engagement against a
//                  deployed environment, not to an in-process test suite.
// ============================================================================
global using FluentAssertions;
global using NSubstitute;
global using Xunit;
