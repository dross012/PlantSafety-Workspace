-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_CyberPhysicalIncident_Update.sql | Created: 2026-08-04
--  Description: Updates Status/ClosedAtUtc/ResponseTimeMinutes on an existing CyberPhysicalIncident
--                row. Backs ICyberPhysicalIncidentRepository.UpdateAsync (lifecycle transitions and
--                CloseCyberPhysicalIncidentCommand).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Update]
    @CyberPhysicalIncidentId UNIQUEIDENTIFIER,
    @TenantId                UNIQUEIDENTIFIER,
    @Status                  NVARCHAR(20),
    @ClosedAtUtc             DATETIME2(3) = NULL,
    @ResponseTimeMinutes     INT = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [CyberPhysicalSecurity].[CyberPhysicalIncident]
        SET Status = @Status,
            ClosedAtUtc = @ClosedAtUtc,
            ResponseTimeMinutes = @ResponseTimeMinutes
        WHERE CyberPhysicalIncidentId = @CyberPhysicalIncidentId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
