# Step G — UI Modernization Readiness Assessment

## 0. Correction to scope framing

**A production UI already exists.** `PlantSafety.UI` is a Blazor Hybrid
application with 269 `.razor` files across ~80 per-domain modules
(`Modules/IncidentManagement`, `Modules/Culture`, `Modules/Compliance`,
`Modules/Scheduling`, `Modules/Maintenance`, etc.), each with its own
`ApiClients/`, `Models/`, `Pages/`, and `State/` folders. "UI
modernization" here is therefore not a greenfield build — it is
continued/expanded work against an existing UI that already assumes
backend functionality exists for the modules it has pages for. This
materially changes the readiness question from "can we start building
UI" to "which parts of the already-built UI are safe to rely on or extend
right now, and which are calling into backend gaps."

## 1. Blockers, by category

### Domain correctness

- **`IncidentManagement`**: the aggregate migration (Step D) is
  substantially sound — invariants, events, and repository behavior are
  grounded and mostly verified, with a small number of open verification
  items (SQL-layer invariant cross-reference, outbox-write atomicity).
  `Modules/IncidentManagement`'s existing UI pages
  (`IncidentsListPage.razor`, `IncidentDetailPage.razor`,
  `IncidentTimelinePage.razor`, `IncidentSlaMonitorPage.razor`) are
  **likely safe** to continue building against, once Steps A/C's
  integration-test-host blocker is cleared enough to actually prove it
  end-to-end rather than by static inspection alone.
- **`Compliance`**: per the extraction blueprint, only 3 of ~124 API
  actions write, and none of those touch the domain aggregate layer —
  `Modules/Compliance`'s UI (if it has write flows — not verified as
  part of this assessment) would be building against an anemic backend
  with the same class of gap `IncidentManagement` had before its Step
  2/3 fix. **Not a hard blocker to read-only UI work**, but a real risk
  for any write-flow UI work in this module until it gets the same
  aggregate treatment.
- **`Analytics.Culture`**: `Modules/Culture/ApiClients/CultureApiClient.cs`
  already exists and presumably calls the Culture API — which, per Step
  A/F, has **zero working stored procedures** today. This is a genuine,
  hard blocker: any Culture UI work beyond static/mock data will fail at
  the database the moment it's exercised, independent of the DI fix
  already in progress.
- **`Scheduling`**: DI registration gap noted in the extraction blueprint
  (Step F, section 4) — narrower than Culture's, since the SQL/aggregate
  layers reportedly exist; `Modules/Scheduling`'s UI readiness depends on
  confirming that registration gap is closed, which is outside this
  assessment's scope to verify.

### Event flows

- Cross-module event flow (`IncidentManagement`/`CorrectiveAction` →
  `Shared.EventBus` → Reporting's read model) has unit/component-level
  test coverage but **no proven end-to-end round trip** (Step D, section
  5) — a UI feature that depends on near-real-time cross-module updates
  (e.g. a dashboard widget showing "recent incidents" sourced from
  Reporting's event-fed read model) should not be considered proven
  reliable until that round trip is actually demonstrated, not just each
  half in isolation.

### Auth

- PlantSafety.Api's authorization tiers (Phase 1) are implemented and
  tested (723/723). `PlantSafety.UI/Infrastructure/Auth/` exists as a
  folder but its contents were not inspected as part of this assessment
  — **action**: confirm the UI's auth flow actually maps to the four
  canonical `Safety.Tier.*` policies (`Viewer`/`Editor`/`Approver`/`Admin`)
  rather than a stale role model, since Phase 1 found and fixed exactly
  this kind of drift on the API side (`"Platform.Admin"` vs
  `"PlatformAdministrator"`) — the UI could plausibly have its own,
  unaudited copy of the same drift.
- Reporting.Api's auth (Step E) is implemented and gated by a startup
  guard; real tenant `Authority`/`Audience` values are still
  environment-variable placeholders, not yet provisioned for any real
  environment per this assessment's research — **any UI work that
  depends on Reporting.Api endpoints will not authenticate successfully
  in a real deployment until that provisioning happens** (this is an ops
  task, not a code gap — see Step E's rollout plan).

### SQL shape

- Per Step F: the schema is broadly stable and consistently conventioned,
  but has confirmed hard gaps (Culture) and soft gaps (Compliance's
  write path, the TRY/CATCH coverage split) that directly map to UI
  module readiness per section 1 above — the SQL assessment and the UI
  readiness assessment are two views of the same underlying set of
  problems, not independent findings.

## 2. Readiness checklist, per module family

| Module family | Backend readiness | Recommended UI stance |
|---|---|---|
| `IncidentManagement` | Aggregate migrated, mostly verified (Step D) | Safe to continue, pending Step C's test-host restoration for final proof |
| `ContinuousImprovement` (CAPA) | Aggregate exists (Phase 2), RowVersion bug fixed (Step B) | Safe to continue, same caveat as above |
| `Analytics.Culture` | **No working stored procedures at all** (Step A) | **Do not build new write-flow UI here until Step A's implementation (already in progress) lands and is proven against a real database** |
| `Compliance` | Anemic write path — SQL/read exist, domain writes mostly bypassed | Read-only UI safe; write-flow UI should wait for a `Compliance` aggregate treatment analogous to `IncidentManagement`'s |
| `Scheduling` | DI registration gap reported, not independently confirmed here | Confirm the gap is closed before relying on write flows |
| `Reporting`-sourced dashboards/widgets | Read model exists with snapshot test coverage, but no proven event-bus round trip | Safe to build against Reporting's *existing* data; treat "real-time updates from a fresh Incident/CAPA event" as unproven until Step D's end-to-end test exists |
| Every other module not touched this session | **Not assessed** — no claim of readiness or blockage is made here for modules outside this session's direct findings (e.g. `Digital Twin`, `Certifications`, `Permits`, most of the ~80 module list) | Apply the same category of checks (domain correctness, event flows, auth, SQL shape) before assuming readiness |

## 3. Gating criteria — what "safe to begin" should actually mean

Before treating a given module's UI work as low-risk, confirm:

1. **A working, non-EF, stored-procedure-backed persistence path exists**
   for every operation the UI needs (verified by direct inspection, the
   way Step A found Culture's gap — not by assuming the API client
   compiling means the backend works).
2. **The domain layer, if one exists for that module, is not anemic** —
   i.e. writes go through aggregate invariants, not directly from
   handler to SQL with no validation (the pattern already fixed for
   `IncidentManagement`, still open for `Compliance`).
3. **Auth policy names used by the UI match the current canonical
   `Safety.Tier.*`/`SafetyRoles` constants**, not a pre-Phase-1 role
   string that may have been silently deprecated.
4. **If the UI feature depends on cross-module event propagation**,
   that propagation has been proven end-to-end, not just unit-tested in
   each half separately.
5. **If the UI targets `Reporting.Api`**, real `AzureAd:Authority`/
   `Audience` values are provisioned in the target environment (Step E)
   — otherwise the feature will work in a demo/mocked context and fail
   the moment it's pointed at a real deployment.

## 4. Overall recommendation

**Partially ready, module-dependent — not a blanket go/no-go.** The
`IncidentManagement`/`CorrectiveAction` line of work this session focused
on is in reasonable shape to build UI against, with the caveat that full
end-to-end proof is still blocked by the unrelated integration-test-host
issue (Steps A/C). `Analytics.Culture` is a hard stop for new write-flow
UI work until its backend gap closes. `Compliance` and `Scheduling` need
their own module-specific verification before their existing UI modules'
write flows should be trusted. The ~75 other UI modules this session did
not touch carry an unknown status and should not be assumed ready or
blocked in either direction without applying the same checklist (section
3) to each.
