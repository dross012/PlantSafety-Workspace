# Phase 5 CI/CD — Existing Config and Gaps

## What actually exists

Searched for `.github/workflows`, `azure-pipelines*.yml`, and similar under
`D:\PlantSafety\source`. Findings:

1. **No CI/CD config was found for the `PlantSafety` solution itself.** There is no
   `.github\workflows`, `azure-pipelines.yml`, or equivalent under
   `D:\PlantSafety\source\PlantSafety`. `PlantSafety\build\*.ps1.txt` (`Build.ps1.txt`,
   `Test.ps1.txt`, `Deploy.ps1.txt`, `MigrateDatabase.ps1.txt`, etc.) are local deployment/build
   scripts, not pipeline definitions, and are not wired to any discovered trigger config.

2. **`SafetyTooling\BuildPipelines`** is a real, generic CI/CD template project (this is
   "SafetyBuild" from the Handbook's tool naming) with:
   - `Pipelines\GitHub\safetybuild.yml`, `safetybuild-pr.yml`, `safetybuild-release.yml` +
     `Pipelines\GitHub\Templates\steps-{restore,build,test,run-safetybuild,publish}.yml`
   - An Azure DevOps mirror under `Pipelines\AzureDevOps\` with the same file set.
   - `Config\safetybuild.matrix.json` (OS matrix: `ubuntu-latest`/`windows-latest`, `dotnet 8.0.x`)
     and `Pipelines\Shared\{matrix,variables,triggers,notifications}.yml`.
   - `GitHooks\pre-commit.txt`, `pre-push.txt`, `drift-check.ps1`/`.sh`,
     `safetybuild-check.ps1`/`.sh` — local git-hook-based drift/SafetyBuild enforcement.

   Inspected the actual step bodies (`steps-restore.yml`, `steps-build.yml`, `steps-test.yml`):
   they run generic `dotnet restore` / `dotnet build -c Release --no-restore` /
   `dotnet test -c Release --no-build --logger trx` against **whatever solution is checked out at
   the repo root** (`safetybuildSolutionPath: "./"` in `Pipelines\Shared\variables.yml`). There is
   **no explicit per-project or per-module matrix list** (e.g. no enumerated array of test project
   names) anywhere in this config — it is solution-wide `dotnet test`, not project-by-project.

3. **`SafetyTooling\.github`** only contains `copilot-instructions.md` — no `workflows\` folder, so
   `SafetyTooling`'s own repo does not currently invoke these pipeline YAML files via a live GitHub
   Actions trigger; they read as a reusable template library meant to be dropped into a consuming
   repo (like `PlantSafety`) rather than an active pipeline for `SafetyTooling` itself.

4. **`Azure\<Service>.Api\pipelines\`** (`Compute.Api`, `Data.Api`, `Eventing.Api`, `Identity.Api`,
   `ML.Api`, `Networking.Api`, `Observability.Api`, ...) each have their own
   `ci\{analyze,build,test}.yml`, `application\{deploy,health,smoke}.yml`,
   `infrastructure\{plan,apply,validate}.yml`. These are infra/deployment pipelines for the Azure
   platform-services layer, not for the `PlantSafety` application solution or its Workflow /
   Compliance / Auditing modules, so they are out of scope for this punch list.

## Punch list / gaps specific to Phase 5

- **No CI pipeline is actually wired up for the `PlantSafety` solution at all.** The only
  functioning `dotnet test`/`dotnet build` automation found in the repo is the generic
  `SafetyTooling\BuildPipelines` template set, and there is no evidence it has been instantiated
  against `PlantSafety.sln` (no `.github\workflows` or `azure-pipelines.yml` inside
  `D:\PlantSafety\source\PlantSafety` references it). This predates Phase 5 and is not something
  Phase 5 introduced — flagging it here since it's the root cause of every other gap below: there
  is currently no automated place for a "run the new Workflow/Compliance/Auditing tests" stage to
  even live.
- **No per-module test project pattern exists to matrix against.** `PlantSafety.Tests` and
  `PlantSafety.IntegrationTests` are single, monolithic test projects organized by module subfolder
  (`PlantSafety.Tests\Compliance\...`, `PlantSafety.IntegrationTests\Compliance\...`), not one test
  project per module. Because of this, the SafetyBuild `steps-test.yml` template's solution-wide
  `dotnet test` would already pick up Phase 5's new `Compliance\RulesEngine`, `Workflow`, and
  `Auditing\AuditReport`/`AuditTimeline` tests automatically once/if the pipeline is actually
  instantiated for `PlantSafety.sln` — there is no explicit test-project name list to edit, so no
  small additive YAML change is applicable here (per this task's own instruction not to author new
  pipeline YAML unless an existing matrix list makes the addition obviously correct by
  pattern-matching; no such list exists).
- **No SQL migration validation stage or event-replay validation stage was found** anywhere in the
  discovered pipeline config (SafetyBuild's `steps-*.yml` set is restore/build/test/run-safetybuild/
  publish only). If/when a real `PlantSafety.sln` pipeline is stood up from the SafetyBuild
  templates, it will need an explicit stage added to validate the new
  `Workflow\Sql\Tables\030-workflow-graph-foreign-keys.sql`,
  `Analytics\UnifiedModel\Sql\Tables\050-fact-table-partitioning.sql`, and
  `Compliance\RulesEngine\Sql\Tables\020-additional-indexes.sql` follow-on scripts apply cleanly in
  order (see `phase5-schema-changes.md`), and a replay-validation stage for the 8 new cross-module
  consumers' idempotency guarantees (see `phase5-event-flow.md`) — neither exists today for any
  module, not just Phase 5's.
- **No performance-regression test hook was found** referencing `PlantSafety.PerformanceTests` from
  any pipeline config, so the new Analytics/Workflow/Compliance/Auditing surfaces are not
  performance-gated in CI today — again a pre-existing gap, not one introduced by Phase 5.

## Recommendation (not actioned in this pass)

Given there is no existing `PlantSafety.sln`-targeted pipeline file to make a small additive change
to, standing up CI for `PlantSafety.sln` (by instantiating the `SafetyTooling\BuildPipelines`
GitHub/Azure DevOps templates against it, then layering a SQL-migration-validation stage and an
event-replay-validation stage) is a real piece of work, not a one-line addition, and was left out of
this documentation pass per the "do not author new pipeline YAML unless obviously safe/additive"
constraint.
