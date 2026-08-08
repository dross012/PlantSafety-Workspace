-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        20-usp-facilityeventorchestrationresult-record.sql
-- Purpose:       Creates Orchestration.usp_FacilityEventOrchestrationResult_Record
--                against [PlantSafety.CoreDB] -- the persisted mirror of
--                InMemoryFacilityEventOrchestrationResultStore.Record(Guid
--                eventId, FacilityEventOrchestrationResult result)
--                (Reporting.Infrastructure\EventBus\ReadModels\
--                InMemoryFacilityEventOrchestrationResultStore.cs).
--
-- IDEMPOTENCY RULE (replicated exactly from the C# store): idempotent on
-- EventId. If a row for @EventId already exists, this is a no-op -- it does
-- NOT overwrite the existing row, matching
-- InMemoryFacilityEventOrchestrationResultStore.Record's
-- "if (!_seen.TryAdd(eventId, 1)) { return; }" early-exit (at-least-once
-- EventBus delivery must never double-count or mutate an already-recorded
-- result).
--
-- Column shape assumption: see 00-create-schema.sql's CONCURRENT WORK NOTE
-- and 10-indexes.sql's ASSUMED COLUMN SHAPE note.
--
-- Idempotent (script itself): CREATE OR ALTER. Safe to re-run.
-- Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_FacilityEventOrchestrationResult_Record]
    @EventId                             uniqueidentifier,
    @TenantId                            nvarchar(100),
    @FacilityId                          uniqueidentifier,
    @SourceModule                        nvarchar(200),
    @EventType                           nvarchar(200),
    @OccurredAtUtc                       datetime2(3),
    @ComplianceRequiredElementsCount     int,
    @ComplianceCompletedElementsCount    int,
    @ComplianceMissingElementsCount      int,
    @CompliancePercentComplete           float,
    @ComplianceTier                      nvarchar(30),
    @ComplianceNotes                     nvarchar(400),
    @RiskHazardLevel                     nvarchar(20),
    @RiskProgramLevel                    nvarchar(20),
    @RiskRegulatorLevel                  nvarchar(20),
    @RiskEventLevel                      nvarchar(20),
    @RiskOverallLevel                    nvarchar(20),
    @Category                            nvarchar(20),
    @RequiresImmediateAttention          bit,
    @Notification                        nvarchar(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        ------------------------------------------------------------
        -- Input validation
        ------------------------------------------------------------
        IF @EventId IS NULL
            THROW 50001, N'EventId is required.', 1;
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        ------------------------------------------------------------
        -- Idempotent-on-EventId insert (no-op if already recorded --
        -- matches InMemoryFacilityEventOrchestrationResultStore.Record
        -- exactly; never overwrites an existing row)
        ------------------------------------------------------------
        IF NOT EXISTS (
            SELECT 1
            FROM [Orchestration].[FacilityEventOrchestrationResult] WITH (UPDLOCK, HOLDLOCK)
            WHERE [EventId] = @EventId
        )
        BEGIN
            INSERT INTO [Orchestration].[FacilityEventOrchestrationResult] (
                [EventId], [TenantId], [FacilityId], [SourceModule], [EventType], [OccurredAtUtc],
                [ComplianceRequiredElementsCount], [ComplianceCompletedElementsCount], [ComplianceMissingElementsCount],
                [CompliancePercentComplete], [ComplianceTier], [ComplianceNotes],
                [RiskHazardLevel], [RiskProgramLevel], [RiskRegulatorLevel], [RiskEventLevel], [RiskOverallLevel],
                [Category], [RequiresImmediateAttention], [Notification], [CreatedUtc]
            )
            VALUES (
                @EventId, @TenantId, @FacilityId, @SourceModule, @EventType, @OccurredAtUtc,
                @ComplianceRequiredElementsCount, @ComplianceCompletedElementsCount, @ComplianceMissingElementsCount,
                @CompliancePercentComplete, @ComplianceTier, @ComplianceNotes,
                @RiskHazardLevel, @RiskProgramLevel, @RiskRegulatorLevel, @RiskEventLevel, @RiskOverallLevel,
                @Category, @RequiresImmediateAttention, @Notification, SYSUTCDATETIME()
            );

            ------------------------------------------------------------
            -- Audit: record insert (best-effort, non-fatal)
            ------------------------------------------------------------
            BEGIN TRY
                INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                    AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
                )
                VALUES (
                    N'Orchestration.FacilityEventOrchestrationResult', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'EventId=', @EventId, N'; TenantId=', @TenantId, N'; FacilityId=', @FacilityId),
                    NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
                );
            END TRY
            BEGIN CATCH
                -- Audit failures must never block primary operation
            END CATCH;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext20 nvarchar(200) = CONCAT(N'Params=EventId=', @EventId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails20 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext20, @ErrorDetails20, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Orchestration.FacilityEventOrchestrationResult', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext20, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
