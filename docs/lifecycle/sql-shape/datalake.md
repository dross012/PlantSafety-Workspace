# SQL Shape Audit — DataLake

Phase 6 of 10. DataLake has no C# domain aggregate layer (structural, inherited) — every "transition"
is a MediatR command handler with a Dapper repository writing directly to a named stored procedure via
`connection.ExecuteAsync`/`ExecuteScalarAsync`, `commandType: CommandType.StoredProcedure`. DataLake
also keeps no `.sql` files under source control in this repository (inherited finding), so unlike every
PlantSafety table in this document set, proc **bodies** are unverifiable here — this phase's shape
check is therefore scoped to what's visible from the C# side: command/DTO field sets, FluentValidation
rules, and the parameter-binding call itself (`DynamicParameters.Add(name, value)` or an explicit
mapper, per `IngestionJobMapper.ToParameters`, directly read this phase).

## Worst issues in this module

1. **DataLake's parameter-binding pattern is fundamentally different from PlantSafety's
   `StoredProcedureBuilder.AddString(name, value, size)` — and does not exhibit the same silent-
   truncation risk.** Direct read of `IngestionRepository.cs` and `IngestionJobMapper.cs` (the
   `CreateIngestionJobAsync` path) shows Dapper's plain `DynamicParameters.Add("@Field", value)` used
   throughout, with **no explicit `size`/`DbType` argument for any string parameter**. Dapper infers the
   parameter's size from the string value's actual length at call time rather than a fixed cap declared
   in C#, so there is no equivalent of `TriggerAlertCommand`'s bug (a command/domain value silently
   truncated to a caller-declared byte size before ever reaching SQL Server). The residual risk this
   pattern does carry is the opposite failure mode: if the underlying (unverifiable) SQL column is
   narrower than the string Dapper sends, SQL Server raises a loud truncation/overflow error rather than
   silently truncating — a real but categorically different, fail-loud risk, not confirmed to have
   actually occurred (no proc body to check the column width against).
2. **`IngestRawDataCommandValidator.cs` (directly read this phase) is markedly more thorough than the
   PlantSafety validators surveyed in prior phases** — `ModuleName` (`MaximumLength(128)` + regex),
   `EventType` (`MaximumLength(256)`), `IngestionPattern` (closed `Must(...)` enum-style check against
   3 literal strings), `PayloadSizeBytes`/`SchemaVersion` (`GreaterThan(0)`), `RequestingUserId`
   (`NotEmpty`) are all present. **This is the strongest command-level validation coverage found in any
   module audited across Phases 1-6** — a positive finding, not a gap.
3. **`IngestionPattern`'s validator (`Must(p => p is "event-based" or "batch" or "streaming")`) is a
   string literal allow-list, not a C# enum** — functionally prevents drift today (any other value fails
   validation before reaching Dapper), but if the SQL side has an actual `CHECK` constraint or lookup
   table with a 4th value (unverifiable, no proc body), the two would silently disagree about what's
   valid without either side throwing — a structural, not confirmed, enum-drift risk, distinct from
   PlantSafety's smart-enum-vs-CHECK-constraint version of the same class of bug.
4. **No transition in this module has ever had its field set matched against an actual SQL column
   list**, because no such list is inspectable — every "Field-Set Match?" answer below is therefore
   scoped to "does the validated command's field set match the parameters the mapper/repository sends,"
   not "...match the destination table." This is the ceiling of what this phase can confirm for
   DataLake, inherited as a structural limitation rather than a new gap.
5. **`RawDataIngestedEvent`/the processing-stage completion events are never instantiated at all**
   (inherited, `publishing-map.md` line 98) — meaning even where the SQL-side shape is confirmed sound,
   there is no event-level signal of what was actually written; the only confirmation a `Create` path
   succeeded is the `Guid` returned by `ExecuteScalarAsync`.

## Method

Direct read of `IngestionRepository.cs`, `IngestionJobMapper.cs`, and
`IngestRawDataCommandValidator.cs` in full (the one Create-path this phase opened end-to-end). The
remaining transitions' parameter-binding code was not individually re-opened this phase; their
validators exist (`DataLake.Application/**/Validators/*.cs`, 24 files confirmed present by directory
listing) but their rule bodies are pattern-inferred from the `IngestRawDataCommandValidator` example
except where noted.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| IngestionJob.Create (Ingest Raw Data) | Yes — `IngestionRequestDto`'s 9 fields map 1:1 to `IngestionJobMapper.ToParameters`'s 11 `@Param` entries (2 extra: `@CorrelationId`/`@NowUtc`, both server-generated, not from the command) | Not verifiable against the SQL column set (no proc body); all DTO fields are non-nullable value types or validated non-empty strings at the C# boundary | Structurally possible on `IngestionPattern` (string allow-list, not enum/CHECK-verified) — worst-issue #3 | **No confirmed truncation pattern** — Dapper's `DynamicParameters.Add` auto-sizes from value length, unlike PlantSafety's fixed-size `AddString` (worst-issue #1) | Directly verified (mapper + validator + repository all read in full) | Strongest-validated Create path found in this document set (worst-issue #2) |
| ProcessingJob.Create (Raw→Cleansed, Cleansed→Curated) | Not individually re-opened this phase | Not verified | Not verified | Pattern-inferred **No** (same repository-base pattern as Ingestion, no `AddString`-style fixed-size binding found anywhere in `DataLakeRepositoryBase.cs` or its subclasses) | Pattern-inferred | Validators exist (`ProcessRawToCleansedCommandValidator.cs`, `ProcessCleansedToCuratedCommandValidator.cs`) per directory listing, not opened |
| IntegrationEvent.Create (Publish Integration Event) | Not individually re-opened | Not verified | Not verified | Pattern-inferred No | Pattern-inferred | Bypasses DataLake's own Outbox — goes straight to in-process MediatR (inherited); shape of what's published vs. what the validator checked not cross-referenced this phase |
| Dataset (Archival).ArchivePartitionAsync | Not individually re-opened | Not verified | Not verified | Pattern-inferred No | Pattern-inferred | `ArchiveDatasetCommandValidator.cs`/`ApplyArchivalTierCommandValidator.cs` exist per directory listing |
| Access Request (Governance).Approve | Not individually re-opened | Not verified | Not verified | Pattern-inferred No | Pattern-inferred | `ApproveDataAccessCommandValidator.cs` exists |
| Dataset ownership/documentation updates | Not opened by this phase or the prior discovery | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed | Still an open follow-up (inherited) — `UpdateDatasetDocumentationCommandValidator.cs`/`UpdateDatasetOwnershipCommandValidator.cs` exist per directory listing but were not read |
