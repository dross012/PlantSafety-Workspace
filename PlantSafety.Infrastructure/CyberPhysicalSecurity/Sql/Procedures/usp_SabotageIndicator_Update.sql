-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SabotageIndicator_Update.sql | Created: 2026-08-04
--  Description: Updates Severity/Status/CorrelatedIndicatorCount/RiskManagementPriority after
--                correlation/escalation or triage. Backs ISabotageIndicatorRepository.UpdateAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_Update]
    @SabotageIndicatorId      UNIQUEIDENTIFIER,
    @TenantId                 UNIQUEIDENTIFIER,
    @SiteId                   UNIQUEIDENTIFIER,
    @Source                   NVARCHAR(30),
    @SourceReferenceId        UNIQUEIDENTIFIER = NULL,
    @Description              NVARCHAR(2000),
    @Severity                 NVARCHAR(20),
    @Status                   NVARCHAR(30),
    @CorrelatedIndicatorCount INT,
    @RaisedAtUtc              DATETIME2(3),
    @RiskManagementPriority   NVARCHAR(5),
    @CreatedAtUtc             DATETIME2(3),
    @CreatedBy                UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [CyberPhysicalSecurity].[SabotageIndicator]
        SET Severity = @Severity,
            Status = @Status,
            CorrelatedIndicatorCount = @CorrelatedIndicatorCount,
            RiskManagementPriority = @RiskManagementPriority
        WHERE SabotageIndicatorId = @SabotageIndicatorId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
