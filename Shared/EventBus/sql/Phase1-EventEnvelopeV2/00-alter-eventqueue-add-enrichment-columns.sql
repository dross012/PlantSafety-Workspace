-- =============================================
-- Shared.EventBus
-- Script:        00-alter-eventqueue-add-enrichment-columns.sql
-- Purpose:       Phase B.4 (Path B: Transformative Modernization) -- extends the existing
--                dbo.EventQueue table (see Phase0-Tables/00-create-eventqueue-table.sql) with
--                four nullable enrichment columns, so EventEnvelopeV2 can be durably published
--                through the SAME physical table V1 already uses, rather than a parallel queue.
--
--                Columns are NULLable with no default: every row published by the existing V1
--                SqlEventPublisher (which never sets them) simply has NULL enrichment, exactly
--                as it does today. No existing row, INSERT statement, or SELECT with an explicit
--                column list (every V1 query in this codebase already uses one -- see
--                SqlEventSubscriber.cs's DueEventsSql) is affected by this change.
--
-- OWNERSHIP:     Shared.EventBus owns dbo.EventQueue (see the base table script's own
--                OWNERSHIP note). This migration keeps that ownership unchanged.
--
-- IDEMPOTENT / NON-DESTRUCTIVE / ADDITIVE-ONLY: every column addition is guarded by a
-- COL_LENGTH existence check. No ALTER/DROP of any existing column, and no NOT NULL column
-- added without a default (there are none here).
-- =============================================

IF COL_LENGTH(N'dbo.EventQueue', N'FacilityId') IS NULL
BEGIN
    ALTER TABLE dbo.EventQueue ADD FacilityId uniqueidentifier NULL;
END
GO

IF COL_LENGTH(N'dbo.EventQueue', N'IndustryName') IS NULL
BEGIN
    ALTER TABLE dbo.EventQueue ADD IndustryName nvarchar(200) NULL;
END
GO

IF COL_LENGTH(N'dbo.EventQueue', N'Location') IS NULL
BEGIN
    ALTER TABLE dbo.EventQueue ADD Location nvarchar(500) NULL;
END
GO

IF COL_LENGTH(N'dbo.EventQueue', N'HazardType') IS NULL
BEGIN
    ALTER TABLE dbo.EventQueue ADD HazardType nvarchar(50) NULL;
END
GO

-- Filtered index: FacilityId is expected to stay NULL for the large majority of rows for a
-- long while (see Shared.EventBus.Abstractions.Contracts.FacilityInferenceRules.cs's header --
-- no real tenant-to-facility binding is populated anywhere yet). A filtered index keyed only on
-- the non-NULL subset stays small and useful once real enrichment starts flowing, instead of
-- indexing a column that is NULL in nearly every row.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EventQueue_FacilityId' AND object_id = OBJECT_ID(N'dbo.EventQueue'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EventQueue_FacilityId
        ON dbo.EventQueue (FacilityId, EnqueuedAtUtc)
        WHERE FacilityId IS NOT NULL;
END
GO
