-- =============================================
-- Phase 8 Structural Additions
-- Script:        82-sequences-human-readable-numbers.sql
-- Purpose:       Add SQL Server SEQUENCE-backed deterministic number
--                generators for the real, currently-just-a-string "Number"
--                columns confirmed live in PlantSafety.CoreDB's Compliance
--                schema on 2026-07-19:
--                  - Compliance.Permits.PermitNumber            nvarchar(200) NOT NULL
--                  - Compliance.Certification.CertificationNumber nvarchar(200) NOT NULL
--                  - Compliance.Procedures.ProcedureNumber        nvarchar(200) NOT NULL
--
-- No-Guessing Rule compliance: Compliance.Auditing (the "Auditing" table
-- explicitly named in the task prompt) was checked live and has NO
-- Number/Reference-style column at all (only AuditTitle, AuditType, Scope,
-- AuditorId, ScheduledAtUtc, CompletedAtUtc, FindingsCount, Score,
-- ComplianceRating). No sequence was invented for it. Likewise
-- SafetyEvents.IncidentManagement was checked live and has no
-- IncidentNumber-style column (StrongId + Title/Description only) -- no
-- incident-numbering sequence was invented, despite it being the most
-- obvious "enterprise-completeness" candidate, because there is no real
-- column to ground it in.
--
-- Design:
--   - One SEQUENCE per entity (bigint, START WITH 1, no reset-on-year-
--     change logic). NEXT VALUE FOR cannot be used inside a scalar
--     function, view, check constraint, or default (verified live against
--     this instance -- Msg 11719), so each sequence is paired with a
--     generator STORED PROCEDURE (OUTPUT parameter), not a function.
--   - Format: '<Prefix>-<CalendarYear>-<6-digit zero-padded sequence
--     value>', e.g. 'PMT-2026-000001'. The numeric suffix is a lifetime
--     monotonic counter (does NOT reset every calendar year) -- this is a
--     deliberate simplification to guarantee global uniqueness without the
--     extra per-year-reset bookkeeping (tracking last-seen year, handling
--     concurrent rollover, etc.); the embedded year in the string is purely
--     for human readability, matching how CorrelationId/EventHash-style
--     values are already presented for readability elsewhere in this
--     schema. If true per-year-reset numbering is required later, that is
--     a deliberate policy decision to raise with the user, not something
--     to guess at here.
--   - Proc naming follows the canonical sp_<Entity>_<Operation> convention;
--     Operation = GenerateNumber.
--   - Error logging only (mandatory, per canonical template); no
--     AuditLog "viewer access" entry is written by the generator procs
--     themselves -- number generation is an internal helper step within a
--     larger Create operation (which already performs its own audit
--     logging), not itself a standalone entity CRUD/viewer-access event.
--
-- Idempotent: CREATE SEQUENCE is guarded by sys.sequences existence check;
-- procedures use CREATE OR ALTER. Safe to re-run -- re-running will NOT
-- reset or duplicate any already-issued sequence value. All application
-- tables confirmed at 0 rows before and after (no data was numbered by
-- this script; it only builds the generator infrastructure).
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

------------------------------------------------------------------------------
-- Compliance.Permits.PermitNumber  ('PMT-YYYY-######')
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'Seq_PermitNumber' AND SCHEMA_NAME(schema_id) = N'Compliance')
    CREATE SEQUENCE [Compliance].[Seq_PermitNumber] AS bigint START WITH 1 INCREMENT BY 1 MINVALUE 1 NO CYCLE;
GO

-- =============================================
-- Procedure:     [Compliance].sp_Permits_GenerateNumber
-- Description:   Issues the next deterministic, human-readable PermitNumber
--                ('PMT-YYYY-######') from Compliance.Seq_PermitNumber.
-- Notes:
--   - Mandatory error logging with contextual parameters.
--   - XACT_ABORT ensures rollback on error.
--   - NEXT VALUE FOR cannot be used in a scalar function (SQL Server Msg
--     11719) -- this is intentionally a stored procedure.
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Permits_GenerateNumber
    @PermitNumber nvarchar(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @__NextVal bigint = NEXT VALUE FOR [Compliance].[Seq_PermitNumber];
        SET @PermitNumber = CONCAT(N'PMT-', DATEPART(YEAR, SYSUTCDATETIME()), N'-', RIGHT(CONCAT(N'000000', CAST(@__NextVal AS nvarchar(20))), 6));
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        THROW;
    END CATCH
END
GO

------------------------------------------------------------------------------
-- Compliance.Certification.CertificationNumber  ('CERT-YYYY-######')
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'Seq_CertificationNumber' AND SCHEMA_NAME(schema_id) = N'Compliance')
    CREATE SEQUENCE [Compliance].[Seq_CertificationNumber] AS bigint START WITH 1 INCREMENT BY 1 MINVALUE 1 NO CYCLE;
GO

-- =============================================
-- Procedure:     [Compliance].sp_Certification_GenerateNumber
-- Description:   Issues the next deterministic, human-readable
--                CertificationNumber ('CERT-YYYY-######') from
--                Compliance.Seq_CertificationNumber.
-- Notes: same conventions as sp_Permits_GenerateNumber above.
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Certification_GenerateNumber
    @CertificationNumber nvarchar(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @__NextVal bigint = NEXT VALUE FOR [Compliance].[Seq_CertificationNumber];
        SET @CertificationNumber = CONCAT(N'CERT-', DATEPART(YEAR, SYSUTCDATETIME()), N'-', RIGHT(CONCAT(N'000000', CAST(@__NextVal AS nvarchar(20))), 6));
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        THROW;
    END CATCH
END
GO

------------------------------------------------------------------------------
-- Compliance.Procedures.ProcedureNumber  ('PROC-YYYY-######')
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'Seq_ProcedureNumber' AND SCHEMA_NAME(schema_id) = N'Compliance')
    CREATE SEQUENCE [Compliance].[Seq_ProcedureNumber] AS bigint START WITH 1 INCREMENT BY 1 MINVALUE 1 NO CYCLE;
GO

-- =============================================
-- Procedure:     [Compliance].sp_Procedures_GenerateNumber
-- Description:   Issues the next deterministic, human-readable
--                ProcedureNumber ('PROC-YYYY-######') from
--                Compliance.Seq_ProcedureNumber.
-- Notes: same conventions as sp_Permits_GenerateNumber above.
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Procedures_GenerateNumber
    @ProcedureNumber nvarchar(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @__NextVal bigint = NEXT VALUE FOR [Compliance].[Seq_ProcedureNumber];
        SET @ProcedureNumber = CONCAT(N'PROC-', DATEPART(YEAR, SYSUTCDATETIME()), N'-', RIGHT(CONCAT(N'000000', CAST(@__NextVal AS nvarchar(20))), 6));
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        THROW;
    END CATCH
END
GO
