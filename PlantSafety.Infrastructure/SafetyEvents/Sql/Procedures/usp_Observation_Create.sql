-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Observation_Create.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [SafetyEvents].[Observation] row. Backs
--                  IObservationRepository.CreateAsync / ObservationDapperRepository.CreateAsync.
--                  Parameter set matches StoredProcedureBuilder.WithCanonicalHeaders plus the
--                  explicit Add* calls in ObservationDapperRepository.CreateAsync exactly. Status
--                  starts fixed at 'Recorded' (see Observation.sql header -- no lifecycle-
--                  transition surface exists on this feature). TRY/CATCH with AuditDB/ErrorDB
--                  logging per the established pattern. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Observation_Create]
    @TenantId           UNIQUEIDENTIFIER,
    @CorrelationId      UNIQUEIDENTIFIER,
    @ActorId            UNIQUEIDENTIFIER,
    @NowUtc              DATETIME2(3),
    @ObservationId       UNIQUEIDENTIFIER,
    @SiteId              UNIQUEIDENTIFIER,
    @Title               NVARCHAR(200),
    @Description         NVARCHAR(4000),
    @ObservationType     NVARCHAR(30),
    @ObservedAtUtc       DATETIME2(3),
    @ObservedByWorkerId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[Observation]
        (
            ObservationId, TenantId, SiteId, Title, Description, ObservationType, ObservedAtUtc,
            ObservedByWorkerId, Status,
            CreatedAtUtc, CreatedBy, LastModifiedByActorId, LastModifiedAtUtc
        )
        VALUES
        (
            @ObservationId, @TenantId, @SiteId, @Title, @Description, @ObservationType, @ObservedAtUtc,
            @ObservedByWorkerId, N'Recorded',
            @NowUtc, @ActorId, @ActorId, @NowUtc
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Observation', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ObservationId=', @ObservationId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
