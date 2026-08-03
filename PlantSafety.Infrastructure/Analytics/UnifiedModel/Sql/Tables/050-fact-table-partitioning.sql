-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           050-fact-table-partitioning.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Performance Optimization Agent, Phase 5 §7.1)
--  SchemaVersion:  2026.1
--  Description:    FIRST partitioning pattern for this codebase's OLAP star schema
--                  (AnalyticsUnifiedModel). Applies monthly RANGE RIGHT partitioning to the six
--                  highest-volume Phase 5 fact tables: FactSchedulingEvent,
--                  FactOperationalEvent, FactMaintenanceWorkOrder, FactRiskAssessment,
--                  FactContinuousImprovementAction, FactAuditFinding.
--
--  ==========================================================================
--  NEW CONVENTION -- read before adding partitioning to any other table.
--  ==========================================================================
--  This is the second partitioning pass in the codebase; the first (and until now the only
--  precedent) is PlantSafety.Infrastructure\Persistence\Migrations\Phase3\
--  30-partitioning-templates.sql. That pass targeted single-purpose event tables where the
--  natural UTC timestamp column IS the partitioning key. The AnalyticsUnifiedModel fact tables
--  are different: every fact table already carries a column literally named "DateKey", but it
--  is a UNIQUEIDENTIFIER surrogate FK into [AnalyticsUnifiedModel].[DimDate] (see
--  040-fact-dimension-foreign-keys.sql), not an orderable date value -- a GUID surrogate key is
--  NOT usable as a SQL Server RANGE partition function input (values are not sequential/
--  date-correlated, so "monthly" boundaries on it would be meaningless and range elimination
--  would never occur). DO NOT partition on a dimension surrogate key column.
--
--  Instead, each fact table already carries its own real NOT NULL DATETIME2(3) business-event
--  timestamp column, which 030-dashboard-covering-indexes.sql already identified as the actual
--  predicate every dashboard aggregation query filters/groups on:
--      FactRiskAssessment               -> CapturedAtUtc
--      FactAuditFinding                 -> RaisedAtUtc
--      FactMaintenanceWorkOrder         -> DueAtUtc
--      FactOperationalEvent             -> OccurredAtUtc
--      FactSchedulingEvent              -> ScheduledAtUtc
--      FactContinuousImprovementAction  -> OpenedAtUtc
--  These are the correct partitioning keys: real, ascending-over-time, NOT NULL, and already the
--  proven hot-query predicate. CONVENTION: when a table has both a dimension-surrogate
--  "DateKey"-style column and a real business-event timestamp column, partition on the real
--  timestamp column, not the surrogate key -- and prefer whichever timestamp column the table's
--  actual read-path queries already filter/group on (do not guess; check the consuming
--  procs/repositories, as 030-dashboard-covering-indexes.sql did here).
--
--  Boundary scheme: MONTHLY, rolling 36-month window from a fixed epoch (2025-01-01), i.e. 36
--  boundary points from 2025-01-01 through 2027-12-01 inclusive -> 37 partitions per table (one
--  catch-all for anything before the epoch, 36 calendar months of 2025-2027, one trailing
--  catch-all for 2028-01-01 onward). This gives ~16 months of forward headroom past today
--  (2026-08-02). Like the Phase 3 precedent, the trailing catch-all partition will accumulate
--  unbounded future data if left unmaintained -- a recurring ALTER PARTITION FUNCTION ... SPLIT
--  RANGE sliding-window maintenance job (e.g. quarterly, adding the next few months' boundaries
--  and archiving/switching out the oldest) is required on an ongoing basis and is explicitly
--  OUT OF SCOPE for this script; track it separately, same as the Phase 3 precedent flags.
--
--  Approach: same composite-clustered-PK fold as the Phase 3 precedent -- PRIMARY KEY CLUSTERED
--  (<TimestampColumn>, FactId) ON the partition scheme, plus a companion UNIQUE NONCLUSTERED
--  constraint on FactId alone pinned to [PRIMARY] (SQL Server requires a single-column unique
--  index to back any future single-column FK reference to FactId; none exists today per
--  040-fact-dimension-foreign-keys.sql -- these fact tables are FK sources, not FK targets -- but
--  the companion constraint is added defensively to match the established pattern and keep
--  FactId itself always uniquely addressable). All six tables currently have 0 rows in every
--  environment this was authored against, so this is a pure schema change with no data-migration
--  risk. Idempotent (guarded by sys.partition_functions/sys.partition_schemes/sys.indexes
--  existence checks); safe to re-run. Run after 040-fact-dimension-foreign-keys.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Shared monthly boundary list (36 boundaries, 2025-01-01 .. 2027-12-01) -- identical for all six
-- partition functions below; kept as a literal VALUES list per function (SQL Server partition
-- functions cannot share a parameterised boundary list) rather than duplicating via a helper, to
-- match this file's other scripts' plain-T-SQL, no-stored-procedure-helpers convention.

------------------------------------------------------------------------------
-- FactRiskAssessment (CapturedAtUtc)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_FactRiskAssessment')
BEGIN
    CREATE PARTITION FUNCTION PF_FactRiskAssessment (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2025-01-01','2025-02-01','2025-03-01','2025-04-01','2025-05-01','2025-06-01',
        '2025-07-01','2025-08-01','2025-09-01','2025-10-01','2025-11-01','2025-12-01',
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_FactRiskAssessment')
BEGIN
    CREATE PARTITION SCHEME PS_FactRiskAssessment
    AS PARTITION PF_FactRiskAssessment
    ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[AnalyticsUnifiedModel].[FactRiskAssessment]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_FactRiskAssessment')
        ALTER TABLE [AnalyticsUnifiedModel].[FactRiskAssessment] DROP CONSTRAINT PK_FactRiskAssessment;

    ALTER TABLE [AnalyticsUnifiedModel].[FactRiskAssessment]
        ADD CONSTRAINT PK_FactRiskAssessment
        PRIMARY KEY CLUSTERED (CapturedAtUtc, FactId)
        ON PS_FactRiskAssessment(CapturedAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_FactRiskAssessment_FactId')
    ALTER TABLE [AnalyticsUnifiedModel].[FactRiskAssessment]
        ADD CONSTRAINT UQ_FactRiskAssessment_FactId UNIQUE NONCLUSTERED (FactId) ON [PRIMARY];
GO

------------------------------------------------------------------------------
-- FactAuditFinding (RaisedAtUtc)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_FactAuditFinding')
BEGIN
    CREATE PARTITION FUNCTION PF_FactAuditFinding (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2025-01-01','2025-02-01','2025-03-01','2025-04-01','2025-05-01','2025-06-01',
        '2025-07-01','2025-08-01','2025-09-01','2025-10-01','2025-11-01','2025-12-01',
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_FactAuditFinding')
BEGIN
    CREATE PARTITION SCHEME PS_FactAuditFinding
    AS PARTITION PF_FactAuditFinding
    ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[AnalyticsUnifiedModel].[FactAuditFinding]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_FactAuditFinding')
        ALTER TABLE [AnalyticsUnifiedModel].[FactAuditFinding] DROP CONSTRAINT PK_FactAuditFinding;

    ALTER TABLE [AnalyticsUnifiedModel].[FactAuditFinding]
        ADD CONSTRAINT PK_FactAuditFinding
        PRIMARY KEY CLUSTERED (RaisedAtUtc, FactId)
        ON PS_FactAuditFinding(RaisedAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_FactAuditFinding_FactId')
    ALTER TABLE [AnalyticsUnifiedModel].[FactAuditFinding]
        ADD CONSTRAINT UQ_FactAuditFinding_FactId UNIQUE NONCLUSTERED (FactId) ON [PRIMARY];
GO

------------------------------------------------------------------------------
-- FactMaintenanceWorkOrder (DueAtUtc)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_FactMaintenanceWorkOrder')
BEGIN
    CREATE PARTITION FUNCTION PF_FactMaintenanceWorkOrder (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2025-01-01','2025-02-01','2025-03-01','2025-04-01','2025-05-01','2025-06-01',
        '2025-07-01','2025-08-01','2025-09-01','2025-10-01','2025-11-01','2025-12-01',
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_FactMaintenanceWorkOrder')
BEGIN
    CREATE PARTITION SCHEME PS_FactMaintenanceWorkOrder
    AS PARTITION PF_FactMaintenanceWorkOrder
    ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[AnalyticsUnifiedModel].[FactMaintenanceWorkOrder]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_FactMaintenanceWorkOrder')
        ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] DROP CONSTRAINT PK_FactMaintenanceWorkOrder;

    ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder]
        ADD CONSTRAINT PK_FactMaintenanceWorkOrder
        PRIMARY KEY CLUSTERED (DueAtUtc, FactId)
        ON PS_FactMaintenanceWorkOrder(DueAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_FactMaintenanceWorkOrder_FactId')
    ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder]
        ADD CONSTRAINT UQ_FactMaintenanceWorkOrder_FactId UNIQUE NONCLUSTERED (FactId) ON [PRIMARY];
GO

------------------------------------------------------------------------------
-- FactOperationalEvent (OccurredAtUtc)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_FactOperationalEvent')
BEGIN
    CREATE PARTITION FUNCTION PF_FactOperationalEvent (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2025-01-01','2025-02-01','2025-03-01','2025-04-01','2025-05-01','2025-06-01',
        '2025-07-01','2025-08-01','2025-09-01','2025-10-01','2025-11-01','2025-12-01',
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_FactOperationalEvent')
BEGIN
    CREATE PARTITION SCHEME PS_FactOperationalEvent
    AS PARTITION PF_FactOperationalEvent
    ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[AnalyticsUnifiedModel].[FactOperationalEvent]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_FactOperationalEvent')
        ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent] DROP CONSTRAINT PK_FactOperationalEvent;

    ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent]
        ADD CONSTRAINT PK_FactOperationalEvent
        PRIMARY KEY CLUSTERED (OccurredAtUtc, FactId)
        ON PS_FactOperationalEvent(OccurredAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_FactOperationalEvent_FactId')
    ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent]
        ADD CONSTRAINT UQ_FactOperationalEvent_FactId UNIQUE NONCLUSTERED (FactId) ON [PRIMARY];
GO

------------------------------------------------------------------------------
-- FactSchedulingEvent (ScheduledAtUtc)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_FactSchedulingEvent')
BEGIN
    CREATE PARTITION FUNCTION PF_FactSchedulingEvent (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2025-01-01','2025-02-01','2025-03-01','2025-04-01','2025-05-01','2025-06-01',
        '2025-07-01','2025-08-01','2025-09-01','2025-10-01','2025-11-01','2025-12-01',
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_FactSchedulingEvent')
BEGIN
    CREATE PARTITION SCHEME PS_FactSchedulingEvent
    AS PARTITION PF_FactSchedulingEvent
    ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[AnalyticsUnifiedModel].[FactSchedulingEvent]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_FactSchedulingEvent')
        ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent] DROP CONSTRAINT PK_FactSchedulingEvent;

    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent]
        ADD CONSTRAINT PK_FactSchedulingEvent
        PRIMARY KEY CLUSTERED (ScheduledAtUtc, FactId)
        ON PS_FactSchedulingEvent(ScheduledAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_FactSchedulingEvent_FactId')
    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent]
        ADD CONSTRAINT UQ_FactSchedulingEvent_FactId UNIQUE NONCLUSTERED (FactId) ON [PRIMARY];
GO

------------------------------------------------------------------------------
-- FactContinuousImprovementAction (OpenedAtUtc)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = N'PF_FactContinuousImprovementAction')
BEGIN
    CREATE PARTITION FUNCTION PF_FactContinuousImprovementAction (datetime2(3))
    AS RANGE RIGHT FOR VALUES (
        '2025-01-01','2025-02-01','2025-03-01','2025-04-01','2025-05-01','2025-06-01',
        '2025-07-01','2025-08-01','2025-09-01','2025-10-01','2025-11-01','2025-12-01',
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01','2027-02-01','2027-03-01','2027-04-01','2027-05-01','2027-06-01',
        '2027-07-01','2027-08-01','2027-09-01','2027-10-01','2027-11-01','2027-12-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_FactContinuousImprovementAction')
BEGIN
    CREATE PARTITION SCHEME PS_FactContinuousImprovementAction
    AS PARTITION PF_FactContinuousImprovementAction
    ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
    WHERE i.object_id = OBJECT_ID(N'[AnalyticsUnifiedModel].[FactContinuousImprovementAction]') AND i.is_primary_key = 1
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_FactContinuousImprovementAction')
        ALTER TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction] DROP CONSTRAINT PK_FactContinuousImprovementAction;

    ALTER TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction]
        ADD CONSTRAINT PK_FactContinuousImprovementAction
        PRIMARY KEY CLUSTERED (OpenedAtUtc, FactId)
        ON PS_FactContinuousImprovementAction(OpenedAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_FactContinuousImprovementAction_FactId')
    ALTER TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction]
        ADD CONSTRAINT UQ_FactContinuousImprovementAction_FactId UNIQUE NONCLUSTERED (FactId) ON [PRIMARY];
GO
