-- ============================================================================
--  Module:  Alerts.Alert | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Inserts a newly triggered Alert aggregate row.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Trigger]
    @AlertId                         UNIQUEIDENTIFIER,
    @TenantId                        UNIQUEIDENTIFIER,
    @SiteId                          UNIQUEIDENTIFIER = NULL,
    @CategoryCode                    NVARCHAR(50),
    @SeverityCode                    NVARCHAR(20),
    @SourceType                      NVARCHAR(30),
    @Title                           NVARCHAR(200),
    @LocationZoneName                NVARCHAR(200),
    @LocationLatitude                FLOAT = NULL,
    @LocationLongitude               FLOAT = NULL,
    @LocationIndoorPositionReference NVARCHAR(200) = NULL,
    @TriggeredAtUtc                  DATETIME2(3),
    @CreatedBy                       UNIQUEIDENTIFIER = NULL,
    @CorrelationId                   UNIQUEIDENTIFIER,
    @ActorId                         UNIQUEIDENTIFIER,
    @NowUtc                          DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Alerts].[Alert]
            ([AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
             [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude], [LocationIndoorPositionReference],
             [TriggeredAtUtc], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@AlertId, @TenantId, @SiteId, N'Triggered', @CategoryCode, @SeverityCode, @SourceType,
             @Title, @LocationZoneName, @LocationLatitude, @LocationLongitude, @LocationIndoorPositionReference,
             @TriggeredAtUtc, @NowUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
