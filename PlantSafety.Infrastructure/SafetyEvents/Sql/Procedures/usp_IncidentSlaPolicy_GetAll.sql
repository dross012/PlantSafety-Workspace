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

            -- Audited INSIDE the seeding branch deliberately. This procedure is a read that lazily
            -- provisions a tenant's default SLA policies on first access, so auditing at procedure
            -- level would record an INSERT on every read that inserted nothing -- a false trail,
            -- which is worse than no trail at all. TenantId is the whole key here: the defaults are
            -- seeded per tenant as a single set.
            BEGIN TRY
                INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                VALUES (N'IncidentSlaPolicy', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'TenantId=', @TenantId, N'; seeded default SLA policy set'), 0, OBJECT_NAME(@@PROCID));
            END TRY
            BEGIN CATCH
            END CATCH;
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
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
