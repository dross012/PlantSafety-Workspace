# HA/DR Future Work — Always On AGs, Log Shipping, Database Mirroring

**Status:** NOT implemented in this session, by explicit instruction. This document
records exactly what is needed to enable each, and where the hook points are in
this codebase once the prerequisites exist. No live HA/DR topology work was
attempted.

**Current environment (live-verified 2026-07-19):**
- Single instance: `(local)\SQLEXPRESS`, `Dross\SQLEXPRESS`
- Edition: **SQL Server Express Edition (64-bit)**, EngineEdition = 4
- Product version 17.0.4065.4 (SQL Server 2025)
- `SERVERPROPERTY('IsHadrEnabled')` = 0 (Always On not enabled at the instance level)
- No WSFC (Windows Server Failover Cluster) present
- No second SQL Server instance/replica reachable from this box for log
  shipping or mirroring partners
- `BACKUP ... WITH COMPRESSION` itself is blocked on Express (Msg 1844) — see
  `120-backup-jobs-full-diff-log.sql` header comment; this is a smaller,
  separate edition gap from the three HA/DR technologies below, noted here
  because it's in the same "Express edition limits" family.

---

## 1. Always On Availability Groups (AGs)

### What's blocked and why
- AGs require **SQL Server Standard or Enterprise Edition** — not available on
  Express at all (`ALTER AVAILABILITY GROUP` / `CREATE AVAILABILITY GROUP`
  fail outright on Express with "This edition of SQL Server does not support
  Always On Availability Groups").
- AGs require the Windows **Failover Clustering** feature (WSFC) — or, since
  SQL Server 2017+, a **read-scale availability group** without WSFC is
  possible, but that variant provides read-scale only, not automatic
  failover/HA, and still requires ≥2 separate SQL Server instances.
- Requires at least one additional physical or virtual machine (or a second
  named instance on different hardware for the read-scale variant) to host a
  secondary replica.

### What's needed to enable it
1. Upgrade the primary instance (and every replica) to Standard or Enterprise
   Edition.
2. Provision ≥1 additional Windows Server host and install SQL Server
   (Standard/Enterprise) on it as a replica.
3. Install and configure Windows Server Failover Clustering across all nodes
   (or use Enterprise's cluster-less "Basic Availability Group" as a
   Standard-Edition-permitted single-secondary variant, which does exist on
   Standard Edition with a 2-node limit and no readable secondary).
4. Enable the Always On Availability Groups feature via SQL Server
   Configuration Manager (`ALTER SERVER CONFIGURATION SET HADR CLUSTER
   CONTEXT` etc.) and restart the service.
5. Create the availability group, add the databases that are currently FULL
   recovery model candidates (all PlantSafety.* databases are SIMPLE today —
   see below), configure endpoints (mirroring endpoint, certificate or
   Windows auth), and add replicas with synchronous/asynchronous commit mode
   per replica per your RPO/RTO targets.
6. Configure a listener (AG Listener) with a virtual network name/IP for
   client redirection.

### Hook points in this codebase once available
- **Recovery model**: every PlantSafety.* database must be switched from
  SIMPLE to **FULL** before it can join an AG (AGs require FULL recovery).
  The log-backup infrastructure prepped in this session
  (`msdb.dbo.usp_PlantSafety_BackupLog` in `120-backup-jobs-full-diff-log.sql`)
  is already wired up and will start actually backing up logs (instead of
  skipping) the moment recovery model changes to FULL — no code change
  needed there, just the recovery-model ALTER DATABASE statements (a
  deliberate, human-approved governance decision, not made here).
- **Connection strings**: `PlantSafety.Infrastructure` connection-string
  configuration (appsettings / Key Vault secrets referenced elsewhere in the
  solution) would need `MultiSubnetFailover=True` and would point at the AG
  Listener DNS name instead of `(local)\SQLEXPRESS` once a listener exists.
- **TDE cert** (`Phase7-Governance/71-tde-encryption.sql`,
  `Phase7-Governance/tde-cert-backup/PlantSafety_TDE_Cert.cer/.pvk`): if TDE
  is later fully enabled (see Phase 7 notes — cert is prepped but TDE
  encryption itself was left as prep-only), the **same certificate must be
  restored to every AG replica** before the encrypted database can join the
  availability group or fail over. The cert backup files already exist in
  this repo path and should be copied to each replica's master database as
  part of AG setup — this is the direct forward-compatibility hook.
- **Maintenance/backup jobs** (`120`/`121`/`122` in this Phase12-Reliability
  folder): these dynamic, inventory-driven procedures (loop over
  `sys.databases WHERE name LIKE N'PlantSafety.%'`) would need to be
  registered on the **primary replica only** (or wrapped with
  `sys.fn_hadr_backup_is_preferred_replica` / `sys.dm_hadr_database_replica_states`
  checks) once an AG exists, so backup jobs don't run redundantly/incorrectly
  against a secondary. This is a straightforward addition to the job step
  T-SQL (`IF sys.fn_hadr_backup_is_preferred_replica(DB_NAME()) = 1 ...`)
  when the time comes.

---

## 2. Log Shipping

### What's blocked and why
- Log shipping does **not** require Enterprise/Standard edition restrictions
  the way AGs do — it is available on Express as a *primary* in principle,
  but Express **cannot run SQL Server Agent** in the fully-featured sense
  needed for the automated log-shipping Agent jobs (Express has SQL Server
  Agent available starting with certain builds/LocalDB variants, but
  practically log shipping's automated backup/copy/restore jobs are designed
  around Agent, and on a single-instance dev box there is no second instance
  to ship logs *to*).
- Fundamentally, log shipping requires **≥2 separate SQL Server instances**
  (primary + secondary, optionally + monitor) — this environment has exactly
  one usable instance (`(local)\SQLEXPRESS`; the second detected service,
  `MSSQL$SQLEXPRESS01`, was not investigated/targeted per the task's explicit
  single-instance scope, and using it as a log-shipping secondary would be a
  separate, deliberate decision requiring its own confirmation).
- All PlantSafety.* databases are currently **SIMPLE recovery model** (log
  shipping requires FULL or BULK_LOGGED, same prerequisite as AGs).

### What's needed to enable it
1. A second SQL Server instance (can be a lower/same edition than primary —
   log shipping is available on Standard and above; NOT supported at all on
   Express as either primary or secondary role for the automated jobs,
   per Microsoft licensing docs — confirm current-edition licensing docs
   before relying on this).
2. Switch the databases to FULL recovery model (same governance decision
   noted for AGs above).
3. Configure a shared backup share reachable by both instances (or push/pull
   file copy jobs).
4. Use `sp_add_log_shipping_primary_database` /
   `sp_add_log_shipping_secondary_database` (or the Management Studio Log
   Shipping wizard) to set up the backup job (primary), copy job (secondary),
   and restore job (secondary), plus an optional monitor server.

### Hook points in this codebase once available
- The `msdb.dbo.usp_PlantSafety_BackupLog` procedure and its Agent job
  (`120-backup-jobs-full-diff-log.sql`) are **not** a substitute for log
  shipping's own dedicated backup job — log shipping wizard creates its own
  backup job pointing at the shared log-shipping folder. If log shipping is
  set up later, either retire `usp_PlantSafety_BackupLog`'s Agent job for the
  databases enrolled in log shipping (to avoid two competing log-backup jobs
  truncating each other's LSN chain) or repoint log shipping at the same
  backup folder/job — this needs a deliberate integration decision at setup
  time, flagged here so it isn't accidentally double-run.
- The dynamic multi-database design (loop over `sys.databases LIKE
  'PlantSafety.%'`) generalizes well to log shipping's per-database
  configuration — the same inventory query could drive a setup script that
  calls `sp_add_log_shipping_primary_database` for every in-scope database.

---

## 3. Database Mirroring

### What's blocked and why
- Database mirroring is **deprecated** since SQL Server 2016 and Microsoft
  has stated it will be removed in a future version — Microsoft's own
  guidance is to use Always On Availability Groups instead for any new work.
  Not recommended to build net-new even where technically possible.
- Like AGs and log shipping, mirroring requires a **second SQL Server
  instance** (the mirror partner) and, for automatic failover, a third
  instance acting as witness. Not available here (single instance).
- Full safety (synchronous) mirroring requires FULL recovery model — same
  SIMPLE-recovery-model gap as above.

### What's needed to enable it (if ever actually chosen over an AG)
1. A second SQL Server instance to act as mirror partner (any edition that
   supports mirroring — Express can act as a *witness* only, not as a
   principal/mirror partner in synchronous "high safety" mode; check current
   Microsoft docs for exact edition support before relying on this since the
   feature is being phased out).
2. Switch databases to FULL recovery model.
3. Configure mirroring endpoints and `ALTER DATABASE ... SET PARTNER`.

### Recommendation
Given the deprecation status, **do not build mirroring** even once a second
instance/edition becomes available — go straight to Always On Availability
Groups (Basic AG on Standard Edition for a 2-node no-read-scale setup, or
full AG on Enterprise) instead. This section exists only to document why the
option was not considered, per the task's "document what's needed" ask.

---

## Summary table

| Technology | Blocked by | Minimum to unblock |
|---|---|---|
| Always On AGs | Edition (Standard/Enterprise) + WSFC + 2nd instance | Edition upgrade, 2nd host, WSFC, FULL recovery model |
| Log Shipping | 2nd instance; Express not supported for the role; SIMPLE recovery model | 2nd instance (Standard+), FULL recovery model |
| Database Mirroring | Deprecated; 2nd instance; SIMPLE recovery model | Not recommended — use AGs instead when unblocked |

## Carries-forward inventory (already built, reusable once unblocked)

- **TDE certificate** (`Phase7-Governance/tde-cert-backup/PlantSafety_TDE_Cert.cer` /
  `.pvk`) — must be restored to every future AG replica or mirror partner
  before an encrypted database can be added to that topology.
- **FULL recovery model decision point** — not yet made (all 18 databases are
  SIMPLE as of 2026-07-19); this is the single blocking governance decision
  shared by all three technologies above.
- **`msdb.dbo.usp_PlantSafety_BackupLog`** (`120-backup-jobs-full-diff-log.sql`)
  — already dynamic/inventory-driven and already skips SIMPLE-model databases
  safely; will activate automatically the moment recovery model changes,
  requiring no code change, only a per-replica execution scoping decision
  for AGs (see hook-point notes above).
- **Dynamic database-inventory pattern** (`sys.databases WHERE name LIKE
  N'PlantSafety.%' AND state_desc = 'ONLINE' AND database_id > 4`) used
  throughout `120`/`121`/`122`/`123` — reusable as the driver query for any
  future AG-membership, log-shipping-enrollment, or mirroring-setup script.
