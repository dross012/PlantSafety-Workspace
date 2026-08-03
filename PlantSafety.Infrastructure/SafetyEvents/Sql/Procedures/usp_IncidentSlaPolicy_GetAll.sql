-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.SlaMonitoring
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentSlaPolicy_GetAll.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns every persisted SLA policy row for a tenant. Backs
--                  ISlaPolicyRepository.GetAllAsync / SlaPolicyDapperRepository.GetAllAsync. If
--                  the tenant has no rows yet, seeds the platform-default policy set (mirrors
--                  SlaPolicy.Default) before selecting, so the table is self-provisioning on
--                  first use rather than requiring a separate admin-authoring step (none exists
--                  yet for this genuinely new domain). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentSlaPolicy_GetAll]
(
    @TenantId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[IncidentSlaPolicy] WHERE TenantId = @TenantId)
        BEGIN
            INSERT INTO [SafetyEvents].[IncidentSlaPolicy] (TenantId, Severity, AcknowledgeWithinMinutes, ResolveWithinMinutes)
            VALUES
                (@TenantId, N'Critical', 15,  240),
                (@TenantId, N'Serious',  30,  480),
                (@TenantId, N'Moderate', 60,  1440),
                (@TenantId, N'Minor',    240, 4320);
        END

        SELECT
            Severity,
            AcknowledgeWithinMinutes,
            ResolveWithinMinutes
        FROM [SafetyEvents].[IncidentSlaPolicy]
        WHERE TenantId = @TenantId;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
