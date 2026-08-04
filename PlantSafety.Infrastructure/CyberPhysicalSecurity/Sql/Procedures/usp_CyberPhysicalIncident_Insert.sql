-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_CyberPhysicalIncident_Insert.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]
    @CyberPhysicalIncidentId    UNIQUEIDENTIFIER,
    @TenantId                   UNIQUEIDENTIFIER,
    @SiteId                     UNIQUEIDENTIFIER,
    @Title                      NVARCHAR(256),
    @Description                NVARCHAR(4000),
    @Severity                   NVARCHAR(20),
    @Status                     NVARCHAR(20),
    @RelatedSabotageIndicatorIds NVARCHAR(4000) = NULL,
    @OwnerId                    UNIQUEIDENTIFIER,
    @OpenedAtUtc                DATETIME2(3),
    @ClosedAtUtc                DATETIME2(3) = NULL,
    @ResponseTimeMinutes        INT = NULL,
    @CreatedAtUtc                DATETIME2(3),
    @CreatedBy                   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[CyberPhysicalIncident]
        (CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
         OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy)
        VALUES
        (@CyberPhysicalIncidentId, @TenantId, @SiteId, @Title, @Description, @Severity, @Status, @RelatedSabotageIndicatorIds,
         @OwnerId, @OpenedAtUtc, @ClosedAtUtc, @ResponseTimeMinutes, @CreatedAtUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
