# Command-Handler Map — PPE

See `_shared-findings.md` for F7 (Outbox universal), F8 (**all 5 aggregates in this file are on the
23-aggregate SQL-audit list**: `PPEAssignment`, `PPECategory`, `PPEInventory`, `PPETrainingFit`,
`PPEZoneMapping` all confirmed present), F9 (event delivery, [EVT-A] naming collision with
`PPEManagementCreatedEvent`, [EVT-B] otherwise), F10. Command/handler names pattern-inferred; not
individually opened this phase. See `employeesafety.md` for the possibly-overlapping
`EmployeeSafety.Ppe` aggregate (not confirmed as a duplicate).

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| PPEAssignment | Create | `CreatePPEAssignmentCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEAssignment | Assign | `AssignPPECommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEAssignment | RecordFitTest | `RecordPPEFitTestCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Possible overlap with `EmployeeSafety.Ppe.RecordFitTest` — not confirmed |
| PPEAssignment | FlagForReplacement | `FlagPPEForReplacementCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEAssignment | Retire | `RetirePPEAssignmentCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEAssignment | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| PPEInventory | Create | `CreatePPEInventoryCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEInventory | IssueUnits | `IssuePPEUnitsCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEInventory | ReceiveStock | `ReceivePPEStockCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEInventory | FlagForReplacement | `FlagPPEInventoryForReplacementCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| PPEInventory | Retire | `RetirePPEInventoryCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Best-guarded of the PPE group (6 guard sites, inherited) |
| PPEInventory | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| PPECategory | Create only | `CreatePPECategoryCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Dormant `_Update` proc, unused |
| PPETrainingFit | Create only | `CreatePPETrainingFitCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Dormant `_Update` proc, unused |
| PPEZoneMapping | Create only | `CreatePPEZoneMappingCommand` | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Dormant `_Update` proc, unused |

**Event delivery reality (F9):** `PPEManagementCreatedConsumer` (`publishing-map.md` line 77/
`consumption-map.md` line 40) is bound to `PPEManagementCreatedEvent` — most likely an
Operational-namespace marker's event, not any of these 5 aggregates' own Created events (not
individually disambiguated this phase). No consumer of any kind was found for any of the 5
aggregates' transitions here. Net: zero live delivery for every row in this table.

**Audit summary:** all 5 aggregates are F8-covered — every transition in this module writes a
best-effort/anonymous `AuditLog` row, matching Scheduling and Observability as the best-covered
feature areas by that one metric.
