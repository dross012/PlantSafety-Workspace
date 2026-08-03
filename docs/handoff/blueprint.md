# Handoff-Optimized Modernization Plan (No EF Core, Dapper-centric)

Root: `C:\Users\dross\source`. Saved: 2026-07-26.

This document captures the handoff-oriented modernization plan for the solution. Each numbered
section below is a **process**, not a file — it describes a repeatable activity and the artifacts
it produces on demand. Sections 1–2 (prior context) are referenced but not reproduced here; this
copy starts at Section 3 as provided during handoff.

---

## 3. Column-shape audits

Process, not a file. Compares the shapes across:

- Live SQL Server schema
- Stored procedures + Dapper result models
- DTOs (request/response)
- API contracts (OpenAPI / controllers)
- Domain models (aggregates, value objects)

**Artifacts (generated on demand):**
- mismatch reports
- shape diffs
- migration recommendations

---

## 4. Module-by-module modernization

Process, not a file. Structured rewrite of each module's full surface area:

- Commands
- Queries
- Events
- Handlers
- Policies
- Models
- Repositories (Dapper + stored procedures)
- Controllers

**Artifacts (generated on demand):**
- rewritten code
- modernization checklists
- module status reports

---

## 5. Domain rule enforcement

Process, not a file. Extraction and enforcement of business rules:

- invariants
- constraints
- validation rules
- cross-module rules
- lifecycle rules

**Artifacts (generated on demand):**
- rule catalogs
- enforcement code
- domain service updates

---

## 6. Snapshot baselines

Process, not a file. Creation of deterministic snapshots for integration tests.

**Artifacts (generated on demand):**
- baseline JSON files
- snapshot directories
- test harness updates

---

## 7. Integration test scaffolding

Process, not a file. Structured creation of:

- test fixtures
- test hosts
- test data builders
- snapshot validators
- module test suites

**Artifacts (generated on demand):**
- test code
- scaffolding directories
- baseline files

---

## 8. Docs folder build-out

Process, not a file. Structured creation of:

- architecture docs
- module docs
- migration docs
- lifecycle docs
- ingestion docs
- rule catalogs

**Artifacts (generated on demand):**
- markdown files
- diagrams
- inventories

---

## 9. Predictive scheduling

Process, not a file. Design + implementation of:

- scheduling rules
- forecasting models
- orchestration flows
- dashboard endpoints

**Artifacts (generated on demand):**
- domain models
- rules engine code
- API endpoints
- docs

---

## 10. Audit report printing

Process, not a file. Structured creation of:

- PDF templates
- rendering logic
- layout definitions
- performance tuning
- printing endpoints

**Artifacts (generated on demand):**
- code
- templates
- docs

**Status:** In progress — see [`Reporting/docs/`](../../Reporting/docs) for module-specific output
as this section is executed.
