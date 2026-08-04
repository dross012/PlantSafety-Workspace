-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_ControlChangeEvent_ReclassifyAuthorization.sql | Created: 2026-08-04
--  Description: Updates ONLY the AuthorizationStatus column of an existing ControlChangeEvent row
--                -- the sole mutable field on this otherwise append-only, tamper-evident record.
--                Backs IControlChangeEventRepository.UpdateAsync / FlagUnauthorizedControlChangeCommand.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_ReclassifyAuthorization]
    @ControlChangeEventId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @AuthorizationStatus  NVARCHAR(20)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [CyberPhysicalSecurity].[ControlChangeEvent]
        SET AuthorizationStatus = @AuthorizationStatus
        WHERE ControlChangeEventId = @ControlChangeEventId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
