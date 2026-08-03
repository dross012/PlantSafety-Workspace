-- =============================================
-- Shared.SafetyData
-- Script:        02-usp-get-corrective-actions.sql
-- Purpose:       Creates dbo.usp_GetCorrectiveActions against [PlantSafety.CoreDB],
--                the single canonical, parameterized, read-only query path over
--                [ContinuousImprovement].[CAPA] for Shared.SafetyData.
--                Infrastructure's SafetyDataDapperRepository.
--
-- TABLE PROVENANCE: [ContinuousImprovement].[CAPA] has a real, tracked CREATE
-- TABLE script -- PlantSafety.Infrastructure\ContinuousImprovement\Sql\Tables\
-- CAPA.sql -- backing ICAPARepository.GetActionsAsync via the existing
-- [ContinuousImprovement].[usp_CAPA_GetActions] procedure (\Procedures\
-- usp_CAPA_GetActions.sql). Column names below are copied verbatim from that
-- table script.
--
-- JUDGMENT CALL (flagged, see this module's docs\README.md and
-- Shared.SafetyData.Abstractions\Models\CorrectiveActionEntry.cs for the full
-- rationale): PlantSafety.Infrastructure also has a second, separate
-- corrective-action concept -- CorrectiveActionDapperRepository, calling
-- [ContinuousImprovement].[usp_CorrectiveAction_Create]/[_GetById]/[_List]
-- against an implied [ContinuousImprovement].[CorrectiveAction] table -- but
-- that table/those procedures have NO tracked CREATE TABLE/CREATE PROCEDURE
-- script anywhere in the repository (only the C# read-model shape is
-- confirmed), so this procedure targets [ContinuousImprovement].[CAPA]
-- instead, since it has a verified, version-controlled schema.
--
-- RESULT SET SHAPE: two result sets, mirroring Shared.AuditLogging's
-- usp_SearchAuditLog: (1) a single-row total count, (2) the requested page of
-- CAPA rows, newest first (by CreatedAtUtc).
--
-- SAFETY: SELECT only. This procedure must never gain an INSERT/UPDATE/DELETE
-- statement -- ContinuousImprovement.CAPA is owned and mutated exclusively by
-- PlantSafety's CAPA workflow.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCorrectiveActions
    @TenantId    uniqueidentifier,
    @SiteId      uniqueidentifier = NULL,
    @FromUtc     datetime2        = NULL,
    @ToUtc       datetime2        = NULL,
    @PageNumber  int              = 1,
    @PageSize    int              = 50
AS
BEGIN
    SET NOCOUNT ON;

    IF @TenantId IS NULL
        THROW 50001, N'TenantId is required.', 1;
    IF @PageNumber IS NULL OR @PageNumber < 1
        THROW 50001, N'PageNumber must be a positive integer.', 1;
    IF @PageSize IS NULL OR @PageSize < 1
        THROW 50001, N'PageSize must be a positive integer.', 1;

    ------------------------------------------------------------
    -- Result set 1: total matching row count
    ------------------------------------------------------------
    SELECT COUNT(*)
    FROM [ContinuousImprovement].[CAPA]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
      AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc);

    ------------------------------------------------------------
    -- Result set 2: the requested page, newest first
    ------------------------------------------------------------
    SELECT
        CapaId, CreatedAtUtc, ActionType, Status, Title, DueDateUtc
    FROM [ContinuousImprovement].[CAPA]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
      AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc)
    ORDER BY CreatedAtUtc DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
