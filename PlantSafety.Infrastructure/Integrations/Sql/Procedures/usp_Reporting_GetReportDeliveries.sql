-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Reporting
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Reporting_GetReportDeliveries.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IReportingRepository.GetReportDeliveriesAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Reporting_GetReportDeliveries]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        DeliveryId,
        TenantId,
        SiteId,
        ReportName,
        RecipientSystem,
        DeliveryChannel,
        Status,
        DeliveredAtUtc,
        InitiatedBy
    FROM [Integrations].[ReportingItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND DeliveredAtUtc >= @FromUtc
      AND DeliveredAtUtc <= @ToUtc
    ORDER BY DeliveredAtUtc DESC;
END
GO
