# Command-Handler Map — PermitToWork (PermitWorkflow, PermitTemplate, PermitExpiration, PermitScheduling)

See `_shared-findings.md` for F7 (Outbox universal), F8 (all 4 aggregates in this file **are** on the
23-aggregate SQL-audit list — `PermitWorkflow`, `PermitTemplate`, `PermitExpiration`,
`PermitScheduling` all confirmed present), F9 (event delivery, [EVT-B]), F10, and
`domain-mapping/_shared-findings.md` F5 (distinct-but-overlapping vs `ContractorSafety.PermitToWork`,
see `contractorsafety.md`). Command/handler names pattern-inferred; not individually opened this
phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler (pattern-inferred) | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| PermitWorkflow | Create | `CreatePermitWorkflowCommand` | matching handler | `AddAsync` | `PermitWorkflowCreatedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| PermitWorkflow | AssessHazards | `AssessPermitWorkflowHazardsCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Guard + non-negative `riskScore` + non-empty `requiredControls` checks (inherited) |
| PermitWorkflow | ImplementControls | `ImplementPermitWorkflowControlsCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitWorkflow | Approve | `ApprovePermitWorkflowCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitWorkflow | Issue | `IssuePermitWorkflowCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitWorkflow | RecordClosureSignature | `RecordPermitWorkflowClosureSignatureCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitWorkflow | Reject | `RejectPermitWorkflowCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Functions as this aggregate's "Cancel" (inherited) |
| PermitWorkflow | Expire | `ExpirePermitWorkflowCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitWorkflow | Archive | `ArchivePermitWorkflowCommand` (existence unconfirmed) | matching handler | `GetByIdAsync` → `UpdateAsync` | **Unconfirmed** — F3 leaves this the 1 of 8 Archive-havers still only pattern-inferred (not directly read) | Unconfirmed | **Y** (SQL fires regardless, F8) | Pattern-inferred | Top remaining open item from F3 |
| PermitWorkflow | Cancel (formal) / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | `Reject` covers the negative path informally |
| PermitTemplate | Create | `CreatePermitTemplateCommand` | matching handler | `AddAsync` | `PermitTemplateCreatedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| PermitTemplate | Publish | `PublishPermitTemplateCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitTemplate | Deprecate | `DeprecatePermitTemplateCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitTemplate | Retire | `RetirePermitTemplateCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PermitTemplate | Archive | `ArchivePermitTemplateCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `PermitTemplateArchivedEvent` — **confirmed raised** (`domain-mapping` F3, direct read) | **Y** | **Y** | Directly verified (event side, inherited F3); command/handler class names pattern-inferred | One of only 3 confirmed non-silent Archive transitions in the whole solution (F3) — best-instrumented Archive in the inventory |
| PermitTemplate | Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| PermitExpiration | Create only | `CreatePermitExpirationCommand` | matching handler | `AddAsync` | `PermitExpirationCreatedEvent` (pattern-inferred) | Y | **Y** | Directly verified (SQL, inherited) | `_Update` proc exists as dormant capacity (inherited) — no domain method calls it yet, so no transition currently exercises the Update-side audit/outbox path |
| PermitScheduling | Create only | `CreatePermitSchedulingCommand` | matching handler | `AddAsync` | `PermitSchedulingCreatedEvent` (pattern-inferred) | Y | **Y** | Directly verified (SQL, inherited) | Same dormant-`_Update`-capacity note as PermitExpiration |

**Event delivery reality (F9 [EVT-B]):** no consumer class exists for any PermitWorkflow/PermitTemplate/
PermitExpiration/PermitScheduling event (Created or non-Created) — grep of `PlantSafety.Infrastructure`
for `Permit*Consumer` under this feature area returns none matching these 4 aggregate names (the
`PermitsCreatedConsumer` in `publishing-map.md` line 71/`consumption-map.md` line 34 is bound to
`PermitsCreatedEvent` from the separate `Compliance.Permits` aggregate, not any of these 4). Net: zero
live delivery for every transition in this file.
