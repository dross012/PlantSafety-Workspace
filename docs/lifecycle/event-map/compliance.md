# Event Map — Compliance (Certification, Compliance, ManagementOfChange, Permits, Policy, Procedures, RegulatoryCalendar, Auditing marker)

See `_shared-findings.md` (F14-F17) and `command-handler-map/compliance.md` (esp. F13 —
`RegulatoryCalendar`'s repository has zero implementations). All 8 are Create-only marker
aggregates. Event bodies directly read this phase (`CertificationCreatedEvent`,
`ComplianceCreatedEvent`, `ManagementOfChangeCreatedEvent`, `PermitsCreatedEvent`,
`PolicyCreatedEvent`, `ProceduresCreatedEvent`, `RegulatoryCalendarCreatedEvent`,
`AuditingCreatedEvent` — all confirmed identity/actor/correlation-only, no exceptions).

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Certification | Create only | Y | `CertificationCreatedEvent` | Y — minimal-but-correct | **N (EVT-A)** | Directly verified | No genuine write proc found (inherited) — Outbox row written regardless |
| Compliance | Create only | Y | `ComplianceCreatedEvent` | Y | **N (EVT-A)** | Directly verified | `usp_ComplianceResources_BulkUpdate` is a plausible-but-unconfirmed real write path (inherited) |
| ManagementOfChange | Create only | Y | `ManagementOfChangeCreatedEvent` | Y | **N (EVT-A)** | Directly verified | |
| Permits (Compliance) | Create only | Y | `PermitsCreatedEvent` | Y | **N (EVT-A)** | Directly verified | Distinct from all 5 `PermitToWork`/`PermitWorkflow`-concept aggregates elsewhere — a 6th "permit" naming collision (inherited) |
| Policy | Create only | Y | `PolicyCreatedEvent` | Y | **N (EVT-A)** | Directly verified | Largest SQL footprint (12 `usp_Policy*` procs) of any create-only aggregate, still no confirmed base-entity write proc |
| Procedures | Create only | Y | `ProceduresCreatedEvent` | Y | **N (EVT-A)** | Directly verified | |
| RegulatoryCalendar | Create only | **Unconfirmed / moot — per F13, `AddAsync` cannot resolve at DI time (zero implementations of `IRegulatoryCalendarRepository`), so the command would fail before reaching the point of constructing this event in most execution paths** | `RegulatoryCalendarCreatedEvent` | Y (payload itself is complete, if construction is ever reached) | **N (EVT-A)** — moot regardless | Directly verified (event body); F13 (inherited) governs whether it's ever actually constructed | The sharpest case in this file: a well-formed event attached to a domain method that likely never completes far enough to raise it |
| Auditing (Compliance marker, duplicate) | Create only | Y | `AuditingCreatedEvent` | Y | **N (EVT-A)** | Directly verified | Duplicate of live `Auditing.AuditEngagement`/`AuditFinding` (see `auditing.md`) |
| All 8 above | Update/Delete/Cancel/status | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** 1 event per aggregate, 1 `AddDomainEvent` call per Create factory
— no possibility of a duplicate in a single-method aggregate.

**Module-level summary:** payloads are minimal-but-complete for all 8 (F14) — the module's real
problems, as `command-handler-map/compliance.md` already established, are persistence-layer (6 of 8
have no confirmed SQL write path, `RegulatoryCalendar` has no C# write-repository at all), not
payload-shaped. All 8 have named, unregistered consumer stubs (F9) — zero live delivery regardless
of whether the underlying Create ever successfully persists.
