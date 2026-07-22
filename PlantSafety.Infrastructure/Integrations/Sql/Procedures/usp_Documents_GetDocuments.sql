-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Documents
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Documents_GetDocuments.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IDocumentsRepository.GetDocumentsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Documents_GetDocuments]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        DocumentId,
        TenantId,
        SiteId,
        SourceSystem,
        FileName,
        DocumentType,
        Status,
        SyncedAtUtc,
        SyncedBy
    FROM [Integrations].[DocumentsItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND SyncedAtUtc >= @FromUtc
      AND SyncedAtUtc <= @ToUtc
    ORDER BY SyncedAtUtc DESC;
END
GO
