# SQL Shape Audit — Alerts

Phase 6 of 10. Covers `Alerts.Alert` (shape #1, the confirmed-live one) per-transition. Shape #2
(`Operational.Alerts.Alert`) and shape #3 are not individually SQL-traced this phase — their SQL cannot
be disambiguated from shape #1's by file-name alone (inherited ambiguity, `module-inventory/alerts.md`);
flagged again below, not resolved.

## Worst issues in this module

1. **`TriggerAlertCommand` has zero validator — `Title`/`LocationZoneName` silently truncate at 200
   chars** (inherited headline finding, re-confirmed this phase by direct file re-read: `AddString("Title",
   aggregate.Title, 200)` / `AddString("LocationZoneName", aggregate.Location.ZoneName, 200)` against
   `NVARCHAR(200)` columns, no `*Validator.cs` file exists for any of the 8 Alerts commands).
2. **`SeverityCode` is `string?` on the command but `NVARCHAR(20) NOT NULL` on the column** — masked only
   because `Alert.Trigger` always resolves a non-null value (`severity ?? effectiveCategory.
   DefaultSeverity`) before the row is built. A caller relying on the command's own nullable contract
   would reasonably (and wrongly) assume the column tolerates null. Directly verified (inherited).
3. **`CloseOutVerification` is domain-required (throws if empty) but the column allows NULL** — the
   conservative-direction mismatch (DB more permissive than the domain invariant) means the schema
   itself doesn't reflect the real business rule. Directly verified (inherited).
4. **`RowVersion` (`ROWVERSION`, SQL-maintained) is never read back into the aggregate or `AlertRow`** —
   a second, independent dead-optimistic-concurrency-column instance (parallel to AccidentManagement's
   `ConcurrencyToken`). Directly verified (inherited).
5. **`CategoryCode`/`SeverityCode` have no CHECK constraint** (only `Status` does, via `CK_Alert_Status`)
   — an invalid code is only caught if `AlertCategory.From`/equivalent throws inside the aggregate, and
   this phase did not re-confirm whether that throw happens *before* or *after* the DB write attempt
   inside `Alert.Trigger`. Pattern-inferred (inherited).

## Method

`Alert.Trigger` traced in full by the Ingestion Discovery's shape-audit; this phase adds the 6
`_StatusTransitions`-routed transitions and 2 child-entity writes, cross-referenced against
`command-handler-map/alerts.md`'s F10 (consolidated non-Create handler pattern) and F8 (Alerts is **not**
one of the 23 AuditLog-covered aggregates — confirmed absent from the F8 list). No new validator files
were found for any of the 7 non-Create Alerts commands either (same `find` sweep that confirmed
`TriggerAlertCommand`'s absence covered the whole `PlantSafety.Application\Alerts\` tree) — **every
non-Create transition below inherits the same zero-validator exposure as Trigger**, not just the one
already-known command.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Trigger (Create) | Yes, all fields present | **No** — `SeverityCode` nullable on command, `NOT NULL` on column, masked by domain fallback | No drift found — `CK_Alert_Status` matches `AlertStatus` enum exactly (positive control) | **Yes — confirmed** (Title/LocationZoneName, 200-char silent truncation, no validator) | Directly verified | Worst-issue #1 |
| Acknowledge | Field set not re-opened this phase (no new user-supplied string fields expected — status-only transition per method signature) | N/A (no string fields) | Same CHECK as above | No — no free-text field involved | Pattern-inferred | Zero validator file exists for this command either |
| Dispatch | Same as Acknowledge | N/A | Same | No | Pattern-inferred | |
| Escalate | `EscalationReason`-shaped field expected (per F15's sibling `AlertEscalatedEvent.EscalationReason`); command/validator not opened this phase to confirm the exact field name or a matching `AddString` size | Unconfirmed | Same | **Unconfirmed — flagged** given zero validators exist anywhere in this module and the sibling `WorkOrderPriorityEscalatedEvent`'s equivalent field elsewhere in the solution is un-length-checked too | Pattern-inferred | Follow-up candidate |
| Resolve | Not re-opened | Unconfirmed | Same | Unconfirmed | Pattern-inferred | |
| Close | Not re-opened (may carry a closure note similar to `CloseOutVerification` below) | Same nullability mismatch as `CloseOutVerification` if it is the same field | Same | Unconfirmed | Pattern-inferred (structure), Directly verified (the `CloseOutVerification` field itself, inherited) | Worst-issue #3 |
| RecordDelivery (child entity) | Field set not re-opened | Unconfirmed | N/A | Unconfirmed — no validator exists for this module at all, so any free-text delivery-channel/detail field carries the same class of risk as Title until individually re-opened | Pattern-inferred | No status guard found in this method (inherited) |
| RecordResponseAction (child entity) | `ActorKind`/`UserId` shape directly verified (inherited, ingestion shape-audit) | Yes (`TakenByUserId UNIQUEIDENTIFIER NULL`, matches optional actor) | Unverified whether `ActorKind` values are checked against any CHECK (none found on this table) | Unconfirmed for any free-text field on this entity | Directly verified (ActorKind shape only) | No status guard, no confirmed `AddDomainEvent` (inherited) |
| Cancel / Delete | N/A — transition does not exist | N/A | N/A | N/A | Directly verified | |
| *(Shape #2/#3 — `Operational.Alerts.Alert`, 9 transitions)* | **Not traced this phase** — SQL cannot be disambiguated from shape #1 by file name; a genuine open question, not a shape-audit finding | N/A | N/A | N/A | Unconfirmed | Top Phase 2/7 priority per `module-inventory/alerts.md` |
