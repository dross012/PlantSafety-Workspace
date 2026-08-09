-- ============================================================================
--  Project:        Enterprise.DataLake -- Phase1-DataLake
--  File:           40-trigger-no-update.sql
--  Created:        2026-08-09
--  Description:    Enforces append-only integrity at the database level:
--                  UPDATE and DELETE are both blocked on
--                  [DataLake].[DataLakeRecord], mirroring [SafetyTaxonomy]'s
--                  40-triggers-block-mutation.sql precedent and this
--                  codebase's validate-append-only rule for genuinely
--                  immutable event-history tables. This table also has no
--                  IsDeleted column (see 01-table-datalakerecord.sql) --
--                  soft-delete does not apply to a data lake's raw records.
--  Idempotent: CREATE OR ALTER. Safe to re-run.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER TRIGGER [DataLake].[trg_DataLakeRecord_NoMutation]
ON [DataLake].[DataLakeRecord]
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50002, N'DataLake.DataLakeRecord is append-only; UPDATE and DELETE are not permitted.', 1;
END
GO
