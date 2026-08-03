# SQL Shape Audit — Azure

Phase 6 of 10. **Per-transition SQL shape analysis does not apply to this module, and this is a
structural fact, not a gap.** Per `module-inventory/azure.md` and `command-handler-map/azure.md`
(inherited from the completed Ingestion Discovery, not re-derived this phase), Azure has no local
PlantSafety-tenant-scoped aggregate, no repository layer, and no database table of its own for any of
its 18 resource-operator transitions (Container App Restart, Cosmos Throughput Update, Azure ML
Training Job Cancel, KeyVault Secret Disable, Service Bus Topic Purge Dead Letter, and 13 further
resource types). Every transition's "persistence" is a live call into Azure's own ARM (Azure Resource
Manager) control plane via the Azure SDK — the state being changed lives in Azure's service fabric, not
in any SQL Server database this solution owns or could shape-audit.

## Why the standard shape checks are inapplicable here

- **Field-Set Match?** — There is no command→SQL-column mapping to check. The comparison this phase
  would otherwise run (DTO/command field set vs. stored-procedure parameter list) has no SQL side to
  compare against; the closest analogue is the command's field set vs. the ARM SDK request object's
  field set, which is a different kind of audit (API-contract validation, not SQL shape) and out of
  scope for this phase.
- **Nullability Match?** — SQL `NULL`/`NOT NULL` column semantics don't exist for an ARM resource
  property; ARM's own schema validation (a Microsoft-owned, out-of-repo contract) is the only
  "nullability" surface, and it is not something this solution's SQL-shape audit can inspect.
- **Enum Drift?** — No SQL `CHECK` constraint or lookup table exists to drift against. If a C# enum
  (e.g. a resource state) diverges from ARM's own accepted values, that would surface as an ARM SDK
  exception at call time, not a silent SQL-side mismatch — a fundamentally different failure mode than
  the CHECK-constraint drift this document set catalogs elsewhere.
- **Truncation Risk?** — `StoredProcedureBuilder.AddString(name, value, size)` and Dapper's
  auto-sizing parameter binding are both SQL Server ADO.NET concerns; the ARM SDK's own request/response
  serialization (JSON over HTTPS) has no equivalent byte-length-truncation failure mode analogous to a
  fixed-size SQL `NVARCHAR` parameter.

## What this module's audit trail actually looks like instead (inherited, not re-verified this phase)

Every transition follows one uniform shape: log → ARM SDK call → in-process MediatR `IPublisher.Publish`
(only after the ARM call succeeds) → one `INotificationHandler<T>` that performs a single `ILogger` line
and nothing else. Azure is the only module in the solution where every raised event reaches real,
running consumer code (18 of 18, per `command-handler-map/azure.md`) — but none of those consumers
performs a domain-meaningful action. There is no SQL `AuditLog` equivalent for Azure transitions (F8 is
explicitly scoped to PlantSafety's own database); "Audit Entry?" for every Azure transition is N/A, not
N, for the same structural reason the shape-audit columns are N/A here.

## One item worth carrying forward from the inherited discovery, despite being out of this phase's scope

`Service Bus Topic (Eventing) → Purge Dead Letter` is an irreversible, data-destroying operation with no
confirmed precondition check anywhere in the 2-of-18 fully-read sample the prior discovery took. This
isn't a shape issue (there's no SQL shape to check), but it remains the single most consequential open
item in this module and is repeated here so it isn't lost by this file's "N/A across the board"
framing.
