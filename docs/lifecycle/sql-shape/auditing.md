# SQL Shape Audit — Auditing

Phase 6 of 10. Covers `Auditing.AuditEngagement`/`AuditFinding` (live). `Compliance.Auditing.Auditing`
(create-only duplicate marker, read-only SQL) is traced in `compliance.md`.

## Worst issues in this module

1. **`CreateAuditEngagementCommand.EngagementTitle` — confirmed truncation risk, 2nd instance in the
   solution beyond `TriggerAlertCommand`.** `CreateAuditEngagementCommandValidator.cs:21` allows
   `MaximumLength(256)`, but `AuditEngagementDapperRepository.cs:93` calls
   `.AddString("EngagementTitle", aggregate.EngagementTitle, 200)`. A title of 201–256 characters
   **passes validation** and is then **silently truncated to 200 chars** by the ADO.NET parameter size
   before reaching SQL Server — unlike `TriggerAlertCommand`, this is not "no validator at all," it is a
   validator that exists but was set looser than the persistence-layer parameter size, a subtly different
   and arguably more dangerous variant (the team believed this field was protected). Directly verified
   (`CreateAuditEngagementCommandValidator.cs`, `AuditEngagementDapperRepository.cs`).
2. **`CreateAuditFindingCommand.FindingTitle` — same bug, 3rd instance.**
   `CreateAuditFindingCommandValidator.cs:22`: `MaximumLength(256)`.
   `AuditFindingDapperRepository.cs:90`: `.AddString("FindingTitle", aggregate.FindingTitle, 200)`.
   Identical 256-vs-200 mismatch. Directly verified.
3. **Neither `AuditEngagement` nor `AuditFinding` gets the cross-DB AuditLog insert** (inherited, F8) —
   a fully guarded, richly-lifecycled state machine (Schedule→Start→RecordFinding→Close, plus
   Remediate/TriggerCapa/Verify on the Finding side) with zero rows in `AuditDB.AuditLog`, unlike the 23
   aggregates (including its own sibling `Accident`) that do get it.
4. **No CHECK constraint found on either aggregate's status-shaped columns** (not individually re-opened
   this phase to confirm the exact column name, but no `Auditing`-prefixed table appeared in the 4-file
   solution-wide CHECK-constraint census) — structurally the same enum-drift exposure as the rest of the
   solution outside Alert/Dashboard/Widget.
5. **`ComplianceRating` (30-char param, per earlier module inventory) vs validator `MaximumLength(64)`**
   on `CloseAuditEngagementCommandValidator.cs:22` — **a 4th confirmed truncation-risk instance**: the
   validator allows up to 64 characters but the param size found in the earlier solution-wide AddString
   sweep for this field is 30. A compliance-rating label like "Substantially Compliant with Exceptions"
   (41 chars) would pass validation and then be silently cut mid-word before the write.

## Method

Direct read this phase: `CreateAuditEngagementCommandValidator.cs`, `CreateAuditFindingCommandValidator.cs`,
`CloseAuditEngagementCommandValidator.cs`, `AuditEngagementDapperRepository.cs`,
`AuditFindingDapperRepository.cs` (`AddString` call sites and sizes). Cross-referenced against the
solution-wide `AddString` field/size sweep (grep across all 453 call sites in `PlantSafety.Infrastructure`)
and the solution-wide `MaximumLength` sweep (grep across all 195 validator files) to locate every
field where the validator's cap exceeds the persistence-layer parameter size — this is the method that
surfaced findings #1/#2/#5 above and is repeated per-module for the rest of this phase's files.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| AuditEngagement.Create | Yes | Not individually re-verified | Structurally possible, no CHECK found | **Yes — confirmed** (`EngagementTitle` 256 validator vs 200 param) | Directly verified | Worst-issue #1 |
| AuditEngagement.Schedule | Field set not re-opened (per F10, routes through consolidated non-Create handler) | Not verified | Same | Not individually re-verified this phase | Pattern-inferred | |
| AuditEngagement.Start | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| AuditEngagement.RecordFinding | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| AuditEngagement.Close | Yes (`ComplianceScore`, `ComplianceRating`) | `ComplianceScore` nullable command field, `InclusiveBetween` guard when present — not cross-checked against column nullability this phase | Same | **Yes — confirmed** (`ComplianceRating` 64 validator vs 30 param) | Directly verified | Worst-issue #5; also inherited F6 (unchecked `ComplianceScore` cross-field reference) |
| AuditEngagement.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| AuditFinding.Create | Yes | Not individually re-verified | Same | **Yes — confirmed** (`FindingTitle` 256 validator vs 200 param) | Directly verified | Worst-issue #2 |
| AuditFinding.Remediate | Yes (`RemediationNotes`) | Not verified | Same | No — validator `MaximumLength(2048)` (`RemediateAuditFindingCommandValidator.cs:21`), param size not confirmed smaller this phase | Directly verified (validator), pattern-inferred (param) | |
| AuditFinding.TriggerCapa | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | Assigns to a user with no confirmed existence-check (same class of gap as F11) |
| AuditFinding.Verify | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| AuditFinding.Archive/Cancel/Delete | N/A | N/A | N/A | N/A | Directly verified | |
