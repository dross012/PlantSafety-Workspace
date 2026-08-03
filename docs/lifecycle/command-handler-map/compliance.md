# Command-Handler Map — Compliance

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 7 aggregates are on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-A]), F10, and **F13 (re-verification:
`RegulatoryCalendar`'s domain repository interface has zero implementations anywhere — worse than
Phase 1's "no confirmed SQL" finding, resolved this phase)**. All 7 are Create-only marker
aggregates. Command/handler names pattern-inferred except `RegulatoryCalendar`, directly verified.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Certification | Create only | `CreateCertificationCommand` | matching handler | `AddAsync` — no genuine write proc found (inherited, only subtype `_Get*` read reports) | `CertificationCreatedEvent` | Y (Outbox row written regardless of whether the underlying SQL call would succeed) | **N** | Directly verified (SQL census, inherited) | |
| Compliance | Create only | `CreateComplianceCommand` | matching handler | `AddAsync` — no genuine write proc found | `ComplianceCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | `usp_ComplianceResources_BulkUpdate` is a plausible-but-unconfirmed real write path (inherited) |
| ManagementOfChange | Create only | `CreateManagementOfChangeCommand` | matching handler | `AddAsync` — no genuine write proc found | `ManagementOfChangeCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | |
| Permits (Compliance) | Create only | `CreatePermitsCommand` | matching handler | `AddAsync` — only `usp_PermitsToWork_List` (read-only) found | `PermitsCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | Distinct from all 5 `PermitToWork`/`PermitWorkflow`-concept aggregates elsewhere — a 6th "permit" naming collision |
| Policy | Create only | `CreatePolicyCommand` | matching handler | `AddAsync` — 12 `usp_Policy*` procs exist, none is a genuine base-entity write proc (inherited) | `PolicyCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | Largest SQL footprint of any create-only aggregate, still no confirmed write path for the base entity |
| Procedures | Create only | `CreateProceduresCommand` | matching handler | `AddAsync` — read-only SQL surface only | `ProceduresCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | |
| RegulatoryCalendar | Create only | `CreateRegulatoryCalendarCommand` (exists per `RegulatoryCalendarFactory`) | Unconfirmed which handler, if any, calls `RegulatoryCalendar.Create` and `IRegulatoryCalendarRepository.AddAsync` | **`AddAsync`/`UpdateAsync` would fail to resolve at DI time — `IRegulatoryCalendarRepository` has zero registered implementations anywhere in the solution (F13, directly verified)** | `RegulatoryCalendarCreatedEvent` (defined; would only be constructed if `Create` is ever actually invoked, which nothing found this phase confirms) | Unconfirmed / moot — if `AddAsync` can't resolve, the command would throw before Outbox flush | **N** | **Directly verified (F13) — the strongest "would fail" finding in this file** | This is worse than a missing stored procedure: the C# **interface has no implementation to inject**, so any call site would fail at startup (missing DI registration) or at first invocation, not silently no-op. The read-only `RegulatoryCalendarRepository` class sharing the name is unrelated (doesn't implement the domain interface) and itself calls 2 stored procedures that don't exist as `.sql` files anywhere (`usp_RegulatoryCalendar_GetById`/`GetAll`) |
| Auditing (Compliance marker, duplicate) | Create only | `CreateAuditingCommand` | matching handler | `AddAsync` — read-only SQL surface (`usp_Auditing_GetLogs` only) | `AuditingCreatedEvent` | Y | **N** | Directly verified (SQL, inherited) | Duplicate of live `Auditing.AuditEngagement`/`AuditFinding` (see `auditing.md`) |
| All 8 above | Update/Delete/Cancel/status | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-A]):** all 8 events have named, unregistered consumer stubs
(`CertificationCreatedConsumer`, `ComplianceCreatedConsumer`, `ManagementOfChangeCreatedConsumer`,
`PermitsCreatedConsumer`, `PolicyCreatedConsumer`, `ProceduresCreatedConsumer`,
`RegulatoryCalendarCreatedConsumer`, `AuditingCreatedConsumer` — `publishing-map.md` lines 67-74/
`consumption-map.md` lines 30-37), all subject to the same unregistered-consumer +
wrong-outbox-CLR-type double defect (F9). Net: zero live delivery for every transition in this
module, **and** for 6 of 8 aggregates the underlying SQL write itself has no confirmed destination,
and for `RegulatoryCalendar` specifically the C# write-repository doesn't even exist to attempt it.

**Module-level bottom line:** Compliance is the single largest concentration of "Create with no
confirmed persistence path" aggregates in the solution (7 of the 11 emerging-createonly-cluster
candidates in `emerging-createonly.md` are Compliance-adjacent or listed here), reinforcing Phase 1's
top finding rather than contradicting it.
