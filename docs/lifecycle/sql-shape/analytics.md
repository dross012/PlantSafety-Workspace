# SQL Shape Audit — Analytics

Phase 6 of 10. Covers the 4 Create-only marker aggregates under `PlantSafety.Domain.Analytics`:
`Culture`, `Engagement`, `PredictiveAnalytics`, `ScoreCards` (per `module-inventory/analytics.md`,
`command-handler-map/analytics.md`). Since none of the 4 has a confirmed write proc, this phase's
question shifts from "does the field set/nullability/enum match" (there is no proc body to match
against) to **"is there any confirmed path by which a `Create` call could ever reach a database row at
all"** — and this phase's own DI-resolution check (new this phase, not inherited) answers that more
sharply than Phase 1 could.

## Worst issues in this module

1. **[NEW THIS PHASE] `ICultureRepository` and `IScoreCardsRepository` have zero implementations
   anywhere in the solution** — same category of finding as `command-handler-map/_shared-findings.md`
   F13 (`RegulatoryCalendar`), independently confirmed here by direct grep of the fully-qualified
   interface namespaces (`PlantSafety.Domain.Analytics.Culture.Interfaces`,
   `PlantSafety.Domain.Analytics.ScoreCards.Interfaces`) against every `.cs` file in the solution
   (excluding worktree scratch copies): only the interface file and factory file themselves reference
   the namespace; no class anywhere implements `AddAsync`/`UpdateAsync` against either interface. **This
   is stronger than "no SQL destination found"** — even if a stored procedure existed, there is no
   registrable class for DI to construct, so a `Create`-then-persist call would fail at the DI-resolution
   stage, not the SQL stage. `Culture`/`ScoreCards` join `RegulatoryCalendar` as the 2nd and 3rd confirmed
   instances of this exact failure mode — see `_shared-findings.md` for the consolidated count across all
   25 files.
2. **A same-named, unrelated read-only class exists for `Culture` and creates false confidence** —
   `PlantSafety.Infrastructure/Analytics/Culture/Persistence/CultureRepository.cs` calls real, existing
   procs (`usp_Culture_GetById`/`usp_Culture_GetAll`) but does **not** implement `ICultureRepository`
   (the domain write interface) at all — it is a bare read-model class with only `GetByIdAsync`/
   `GetAllAsync`. A search for "does Culture have SQL" that stops at "a class named `CultureRepository`
   exists and calls procs" would wrongly conclude the write path is fine. Directly verified (read the
   file in full).
3. **`Engagement` and `PredictiveAnalytics` do have real implementing classes**
   (`EngagementDapperRepository.cs`, `PredictiveAnalyticsDapperRepository.cs` — both directly confirmed
   to implement their respective domain interfaces) — so these 2 are one tier better-off than
   `Culture`/`ScoreCards`: DI would resolve, but `PredictiveAnalytics`'s implementation still has only a
   read proc (`usp_PredictiveAnalytics_List`) confirmed, per Phase 1/`module-inventory/analytics.md` — so
   the call would likely throw a "stored procedure not found" `SqlException` at the SQL layer instead of
   failing earlier at DI. `Engagement`'s own SQL write-path status is unconfirmed this phase (Phase 1
   found no proc under exact/loose match, not independently re-verified this phase's SQL census).
4. **`ScoreCards` (Analytics) truncation/enum/nullability questions are entirely moot** — with no
   implementing class, there is no parameter-binding code to inspect for a truncation pattern in the
   first place; this is a structurally different (and, in a narrow sense, "safer") kind of gap than
   `TriggerAlertCommand`'s silent-truncation bug, because it fails loudly (DI error) rather than silently
   mis-persisting.
5. **All 4 aggregates' events are written to Outbox regardless of the underlying persistence outcome**
   (per F7 — Outbox is a MediatR pipeline behavior firing after the handler returns, agnostic to whether
   the repository call actually succeeded) — meaning if any of these 4 handlers swallows or doesn't
   propagate a DI/SQL exception, a `*CreatedEvent` could theoretically reach the Outbox table describing
   a row that was never persisted. Not confirmed this phase (would require tracing exception propagation
   through the generic MediatR pipeline), flagged as a plausible, not directly verified, consequence.

## Method

Field-set/nullability/enum-drift comparison is **not applicable** for any of the 4 aggregates this
phase — none has a confirmed stored-procedure body to compare a command/domain shape against (Phase 1's
file-name census, re-confirmed by this phase's `module-inventory`/`command-handler-map` reads, found at
most a same-named read-only proc for `PredictiveAnalytics` and an ambiguous naming collision for
`ScoreCards`). This phase's new contribution is the DI-implementation check (item 1 above), run by
grepping each aggregate's fully-qualified `Interfaces` namespace solution-wide and confirming whether any
`.cs` file outside the interface/factory pair itself references it.

| Aggregate | Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Culture (Analytics) | Create | N/A — no implementing class exists to bind parameters | N/A | N/A | N/A — nothing to truncate; fails at DI resolution before any parameter is bound | Directly verified (interface-implementation grep, this phase) | Worst-issue #1/#2 |
| Engagement | Create | N/A — repository implementation exists (`EngagementDapperRepository`) but its target proc's existence is unconfirmed this phase | Unconfirmed | Unconfirmed | Unconfirmed — would need to open `EngagementDapperRepository.cs`'s parameter-binding call, not opened this phase | Pattern-inferred | One tier better than Culture/ScoreCards (DI resolves) but SQL destination still unconfirmed per Phase 1 |
| PredictiveAnalytics | Create | N/A — only a read proc (`usp_PredictiveAnalytics_List`) confirmed; no write proc | N/A | N/A | N/A — would throw `SqlException` (proc not found) before any truncation question applies, assuming a `Create`-routed write proc name was expected | Directly verified (SQL census, inherited) | |
| ScoreCards (Analytics) | Create | N/A — no implementing class exists (worst-issue #1) | N/A | N/A | N/A | Directly verified (interface-implementation grep, this phase) | Also carries the unresolved naming-collision risk with the live `Scorecards.Scorecard` table (inherited) — if any write path is ever added, confirm it does not silently target the wrong table |
