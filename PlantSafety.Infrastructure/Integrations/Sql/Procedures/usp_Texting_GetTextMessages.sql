-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Texting
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Texting_GetTextMessages.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing the root-level
--                  Integrations.Texting.ITextingRepository.GetTextMessagesAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Texting_GetTextMessages]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        MessageId,
        TenantId,
        SiteId,
        RecipientPhoneNumber,
        MessageTemplate,
        GatewayProvider,
        Status,
        SentAtUtc,
        InitiatedBy
    FROM [Integrations].[TextingItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND SentAtUtc >= @FromUtc
      AND SentAtUtc <= @ToUtc
    ORDER BY SentAtUtc DESC;
END
GO
