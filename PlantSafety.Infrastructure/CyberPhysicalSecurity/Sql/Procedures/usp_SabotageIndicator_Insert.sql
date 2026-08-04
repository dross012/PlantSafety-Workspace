-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SabotageIndicator_Insert.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_Insert]
    @SabotageIndicatorId     UNIQUEIDENTIFIER,
    @TenantId                UNIQUEIDENTIFIER,
    @SiteId                  UNIQUEIDENTIFIER,
    @Source                  NVARCHAR(30),
    @SourceReferenceId       UNIQUEIDENTIFIER = NULL,
    @Description             NVARCHAR(2000),
    @Severity                NVARCHAR(20),
    @Status                  NVARCHAR(30),
    @CorrelatedIndicatorCount INT,
    @RaisedAtUtc             DATETIME2(3),
    @RiskManagementPriority  NVARCHAR(5),
    @CreatedAtUtc            DATETIME2(3),
    @CreatedBy               UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[SabotageIndicator]
        (SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
         CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy)
        VALUES
        (@SabotageIndicatorId, @TenantId, @SiteId, @Source, @SourceReferenceId, @Description, @Severity, @Status,
         @CorrelatedIndicatorCount, @RaisedAtUtc, @RiskManagementPriority, @CreatedAtUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
