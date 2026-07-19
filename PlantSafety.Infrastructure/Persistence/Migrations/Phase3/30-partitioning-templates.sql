-- =============================================
-- Phase 3 Architectural Remediation
-- Script:        30-partitioning-templates.sql
-- Purpose:       Apply time-based (UTC date) partitioning to the high-volume
--                append-only event/audit-style tables flagged by the
--                Enterprise Data & Database Architecture Master Document
--                (FY2027 Ed.), Section 18C / Section 37.2:
--                  - PlantSafety.DiagnosticsDB : Diagnostics.DiagnosticEvent
--                  - PlantSafety.DiagnosticsDB : Diagnostics.DiagnosticEventArchive
--                  - PlantSafety.PredictiveDB  : PredictiveSafety.SafetyPrediction
--                  - PlantSafety.IntegrationDB : Integration.IntegrationLogEntry
--                  - PlantSafety.IntegrationDB : Integration.ReplayRecord
--
-- Verified live 2026-07-18 against (local)\SQLEXPRESS before authoring:
--   - All five tables use a random GUID (uniqueidentifier) StrongId as the
--     sole clustered PK key column today (PK_<Schema>_<Entity>), which is
--     NOT partition-aligned.
--   - All five tables carry a NOT NULL UTC datetime2(3) "occurrence" column
--     suitable as a partitioning key (no invented columns):
--       DiagnosticEvent          -> OccurredAtUtc
--       DiagnosticEventArchive   -> ArchivedAtUtc   (archive-specific, distinct
--                                                     from its own OccurredAtUtc,
--                                                     chosen because archival
--                                                     writes/retention sweeps are
--                                                     the actual high-volume
--                                                     access pattern on this table)
--       SafetyPrediction         -> CreatedUtc
--       IntegrationLogEntry      -> TimestampUtc
--       ReplayRecord             -> TimestampUtc
--   - All tables currently have 0 rows (confirmed) and live on the PRIMARY
--     filegroup only; PlantSafety.DiagnosticsDB and PlantSafety.IntegrationDB
--     have NO secondary filegroups provisioned. PlantSafety.PredictiveDB does
--     have FG_HotData/FG_ColdData/FG_Event/FG_Index/FG_Summary provisioned,
--     but SafetyPrediction itself currently lives on PRIMARY. To keep this a
--     pure partitioning change with zero unrequested physical-placement side
--     effects, every partition scheme below maps ALL TO ([PRIMARY]). Moving
--     partitions onto dedicated filegroups (e.g. FG_Event for SafetyPrediction)
--     is a good follow-up but is a separate, out-of-scope decision -- flagged
--     here, not silently done.
--   - FK dependency check (live): DiagnosticEvent.DiagnosticEventId is
--     referenced by FK_Diagnostics_DiagnosticMetadata_DiagnosticEvent and
--     FK_Diagnostics_DiagnosticAttachment_DiagnosticEvent; SafetyPrediction.
--     SafetyPredictionId is referenced by
--     FK_PredictiveSafety_PredictionEvidence_SafetyPrediction. Both are plain
--     NO_ACTION/NO_ACTION FKs. DiagnosticEventArchive, IntegrationLogEntry,
--     and ReplayRecord have no incoming FK references.
--
-- Approach chosen: (a) fold the partitioning column into the clustered PK as
-- a composite key (<DateColumn>, <StrongId>), per the task's option (a).
-- Rationale: this keeps the PK clustered and partition-aligned in one object
-- (satisfies create-clustered-pk.md's "PK must align with partitioning" and
-- validate-partitioning.md's "PK and indexes align with partition scheme"
-- rules directly), and the tables are empty so there is no data-migration
-- risk. Where an existing FK references the bare StrongId column (
-- DiagnosticEvent, SafetyPrediction), a companion UNIQUE NONCLUSTERED
-- constraint on the StrongId column alone is added so the FK can still be
-- re-pointed at a single-column unique key (SQL Server requires the FK
-- target to be backed by a unique index/constraint on exactly the referenced
-- column(s); a composite clustered PK alone cannot serve that purpose for a
-- single-column FK). This is a deliberate, documented deviation from a pure
-- single-column StrongId PK -- it is the standard, expected SQL Server
-- pattern for partitioning a surrogate-keyed table that is also an FK
-- target, and is strictly required for referential integrity to survive the
-- change, not a design shortcut.
--
-- Partition granularity (documented per table, no default applied blindly):
--   MONTHLY  - DiagnosticEvent, SafetyPrediction, IntegrationLogEntry
--              These are the direct write-path event/telemetry/prediction
--              tables (one row per diagnostic run, per ML inference, per
--              integration call) -- the actual highest-frequency write
--              tables in scope. Monthly boundaries keep partitions small
--              enough for efficient sliding-window maintenance (SWITCH OUT
--              + purge/archive) without creating excessive empty-partition
--              overhead.
--   QUARTERLY - DiagnosticEventArchive, ReplayRecord
--              These are downstream/derived append-only tables (post-
--              archival-sweep records, replay/audit records) with lower
--              expected write frequency than their source tables. Monthly
--              granularity here would produce many sparsely-populated
--              partitions; quarterly gives a coarser, still-manageable
--              sliding window matched to actual expected volume.
--
-- Boundary range: RANGE RIGHT FOR VALUES, boundary point belongs to the
-- partition to its right (standard "each boundary = first instant of the
-- next period" pattern). Monthly boundaries cover 2026-01-01 through
-- 2027-12-01 (24 boundaries -> 25 partitions: one catch-all for anything
-- before 2026, 24 calendar months of 2026-2027, and a trailing catch-all for
-- 2028-01-01 onward). Quarterly boundaries cover 2026-01-01 through
-- 2029-10-01 (16 boundaries -> 17 partitions), a longer window appropriate
-- for lower-frequency archival/replay data. Both ranges anchor to the
-- current FY2027 architecture-doc timeframe (today is 2026-07-18) with
-- forward headroom; sliding-window maintenance (periodic ALTER PARTITION
-- FUNCTION ... SPLIT/MERGE RANGE) will be needed on an ongoing basis to keep
-- the trailing catch-all partition from accumulating unbounded future data --
-- that recurring maintenance job is out of scope for this script and should
-- be tracked separately (see validate-partitioning.md: "must support sliding
-- window maintenance").
--
-- Idempotent: every step is guarded (partition function/scheme existence
-- checks; PK-already-on-partition-scheme checks before touching PK/FK
-- constraints). Safe to re-run. All tables confirmed 0 rows before and after.
-- =============================================

------------------------------------------------------------------------------
-- PlantSafety.DiagnosticsDB
------------------------------------------------------------------------------
USE [PlantSafety.DiagnosticsDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---- Partition function/scheme: PF_DiagnosticEvent / PS_DiagnosticEvent (monthly) ----
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_DiagnosticEvent')
BEGIN
    CREATE PARTITION FUNCTION PF_DiagnosticEvent (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_DiagnosticEvent')
BEGIN
    CREATE PARTITION SCHEME PS_DiagnosticEvent
    AS PARTITION PF_DiagnosticEvent
    ALL TO ([PRIMARY]);
END;
GO

-- ---- Partition function/scheme: PF_DiagnosticEventArchive / PS_DiagnosticEventArchive (quarterly) ----
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_DiagnosticEventArchive')
BEGIN
    CREATE PARTITION FUNCTION PF_DiagnosticEventArchive (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2026-01-01','2026-04-01','2026-07-01','2026-10-01',
        '2027-01-01','2027-04-01','2027-07-01','2027-10-01',
        '2028-01-01','2028-04-01','2028-07-01','2028-10-01',
        '2029-01-01','2029-04-01','2029-07-01','2029-10-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_DiagnosticEventArchive')
BEGIN
    CREATE PARTITION SCHEME PS_DiagnosticEventArchive
    AS PARTITION PF_DiagnosticEventArchive
    ALL TO ([PRIMARY]);
END;
GO

-- ---- Diagnostics.DiagnosticEvent: fold OccurredAtUtc into clustered PK ----
-- NOTE: each step below is independently idempotent (not gated behind a single
-- outer IF) so a script that is interrupted partway through can be safely
-- re-run to completion.
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[Diagnostics].[DiagnosticEvent]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Diagnostics_DiagnosticMetadata_DiagnosticEvent')
        ALTER TABLE [Diagnostics].[DiagnosticMetadata] DROP CONSTRAINT FK_Diagnostics_DiagnosticMetadata_DiagnosticEvent;

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Diagnostics_DiagnosticAttachment_DiagnosticEvent')
        ALTER TABLE [Diagnostics].[DiagnosticAttachment] DROP CONSTRAINT FK_Diagnostics_DiagnosticAttachment_DiagnosticEvent;

    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_Diagnostics_DiagnosticEvent')
        ALTER TABLE [Diagnostics].[DiagnosticEvent] DROP CONSTRAINT PK_Diagnostics_DiagnosticEvent;

    ALTER TABLE [Diagnostics].[DiagnosticEvent]
        ADD CONSTRAINT PK_Diagnostics_DiagnosticEvent
        PRIMARY KEY CLUSTERED (OccurredAtUtc, DiagnosticEventId)
        ON PS_DiagnosticEvent(OccurredAtUtc);
END;
GO

-- Companion single-column unique constraint so existing FKs can still target
-- DiagnosticEventId alone. Must be explicitly pinned OFF the partition scheme
-- (ON [PRIMARY]) -- a nonclustered unique index that is left partition-aligned
-- is required by SQL Server to include the partitioning column in its key,
-- which would defeat the point of a single-column unique constraint.
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_Diagnostics_DiagnosticEvent_DiagnosticEventId')
    ALTER TABLE [Diagnostics].[DiagnosticEvent]
        ADD CONSTRAINT UQ_Diagnostics_DiagnosticEvent_DiagnosticEventId UNIQUE NONCLUSTERED (DiagnosticEventId) ON [PRIMARY];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Diagnostics_DiagnosticMetadata_DiagnosticEvent')
    ALTER TABLE [Diagnostics].[DiagnosticMetadata]
        ADD CONSTRAINT FK_Diagnostics_DiagnosticMetadata_DiagnosticEvent
        FOREIGN KEY (DiagnosticEventId) REFERENCES [Diagnostics].[DiagnosticEvent](DiagnosticEventId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Diagnostics_DiagnosticAttachment_DiagnosticEvent')
    ALTER TABLE [Diagnostics].[DiagnosticAttachment]
        ADD CONSTRAINT FK_Diagnostics_DiagnosticAttachment_DiagnosticEvent
        FOREIGN KEY (DiagnosticEventId) REFERENCES [Diagnostics].[DiagnosticEvent](DiagnosticEventId);
GO

-- ---- Diagnostics.DiagnosticEventArchive: fold ArchivedAtUtc into clustered PK ----
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[Diagnostics].[DiagnosticEventArchive]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_Diagnostics_DiagnosticEventArchive')
        ALTER TABLE [Diagnostics].[DiagnosticEventArchive] DROP CONSTRAINT PK_Diagnostics_DiagnosticEventArchive;

    ALTER TABLE [Diagnostics].[DiagnosticEventArchive]
        ADD CONSTRAINT PK_Diagnostics_DiagnosticEventArchive
        PRIMARY KEY CLUSTERED (ArchivedAtUtc, DiagnosticEventArchiveId)
        ON PS_DiagnosticEventArchive(ArchivedAtUtc);
END;
GO

------------------------------------------------------------------------------
-- PlantSafety.PredictiveDB
------------------------------------------------------------------------------
USE [PlantSafety.PredictiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---- Partition function/scheme: PF_SafetyPrediction / PS_SafetyPrediction (monthly) ----
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_SafetyPrediction')
BEGIN
    CREATE PARTITION FUNCTION PF_SafetyPrediction (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_SafetyPrediction')
BEGIN
    CREATE PARTITION SCHEME PS_SafetyPrediction
    AS PARTITION PF_SafetyPrediction
    ALL TO ([PRIMARY]);
END;
GO

-- ---- PredictiveSafety.SafetyPrediction: fold CreatedUtc into clustered PK ----
-- NOTE: each step below is independently idempotent (not gated behind a single
-- outer IF) so a script that is interrupted partway through can be safely
-- re-run to completion.
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[PredictiveSafety].[SafetyPrediction]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PredictiveSafety_PredictionEvidence_SafetyPrediction')
        ALTER TABLE [PredictiveSafety].[PredictionEvidence] DROP CONSTRAINT FK_PredictiveSafety_PredictionEvidence_SafetyPrediction;

    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_PredictiveSafety_SafetyPrediction')
        ALTER TABLE [PredictiveSafety].[SafetyPrediction] DROP CONSTRAINT PK_PredictiveSafety_SafetyPrediction;

    ALTER TABLE [PredictiveSafety].[SafetyPrediction]
        ADD CONSTRAINT PK_PredictiveSafety_SafetyPrediction
        PRIMARY KEY CLUSTERED (CreatedUtc, SafetyPredictionId)
        ON PS_SafetyPrediction(CreatedUtc);
END;
GO

-- Companion single-column unique constraint, pinned OFF the partition scheme
-- (ON [PRIMARY]) for the same reason documented above for DiagnosticEvent.
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_PredictiveSafety_SafetyPrediction_SafetyPredictionId')
    ALTER TABLE [PredictiveSafety].[SafetyPrediction]
        ADD CONSTRAINT UQ_PredictiveSafety_SafetyPrediction_SafetyPredictionId UNIQUE NONCLUSTERED (SafetyPredictionId) ON [PRIMARY];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PredictiveSafety_PredictionEvidence_SafetyPrediction')
    ALTER TABLE [PredictiveSafety].[PredictionEvidence]
        ADD CONSTRAINT FK_PredictiveSafety_PredictionEvidence_SafetyPrediction
        FOREIGN KEY (SafetyPredictionId) REFERENCES [PredictiveSafety].[SafetyPrediction](SafetyPredictionId);
GO

------------------------------------------------------------------------------
-- PlantSafety.IntegrationDB
------------------------------------------------------------------------------
USE [PlantSafety.IntegrationDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---- Partition function/scheme: PF_IntegrationLogEntry / PS_IntegrationLogEntry (monthly) ----
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_IntegrationLogEntry')
BEGIN
    CREATE PARTITION FUNCTION PF_IntegrationLogEntry (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_IntegrationLogEntry')
BEGIN
    CREATE PARTITION SCHEME PS_IntegrationLogEntry
    AS PARTITION PF_IntegrationLogEntry
    ALL TO ([PRIMARY]);
END;
GO

-- ---- Partition function/scheme: PF_ReplayRecord / PS_ReplayRecord (quarterly) ----
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_ReplayRecord')
BEGIN
    CREATE PARTITION FUNCTION PF_ReplayRecord (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2026-01-01','2026-04-01','2026-07-01','2026-10-01',
        '2027-01-01','2027-04-01','2027-07-01','2027-10-01',
        '2028-01-01','2028-04-01','2028-07-01','2028-10-01',
        '2029-01-01','2029-04-01','2029-07-01','2029-10-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_ReplayRecord')
BEGIN
    CREATE PARTITION SCHEME PS_ReplayRecord
    AS PARTITION PF_ReplayRecord
    ALL TO ([PRIMARY]);
END;
GO

-- ---- Integration.IntegrationLogEntry: fold TimestampUtc into clustered PK ----
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[Integration].[IntegrationLogEntry]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_Integration_IntegrationLogEntry')
        ALTER TABLE [Integration].[IntegrationLogEntry] DROP CONSTRAINT PK_Integration_IntegrationLogEntry;

    ALTER TABLE [Integration].[IntegrationLogEntry]
        ADD CONSTRAINT PK_Integration_IntegrationLogEntry
        PRIMARY KEY CLUSTERED (TimestampUtc, IntegrationLogEntryId)
        ON PS_IntegrationLogEntry(TimestampUtc);
END;
GO

-- ---- Integration.ReplayRecord: fold TimestampUtc into clustered PK ----
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[Integration].[ReplayRecord]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_Integration_ReplayRecord')
        ALTER TABLE [Integration].[ReplayRecord] DROP CONSTRAINT PK_Integration_ReplayRecord;

    ALTER TABLE [Integration].[ReplayRecord]
        ADD CONSTRAINT PK_Integration_ReplayRecord
        PRIMARY KEY CLUSTERED (TimestampUtc, ReplayRecordId)
        ON PS_ReplayRecord(TimestampUtc);
END;
GO
