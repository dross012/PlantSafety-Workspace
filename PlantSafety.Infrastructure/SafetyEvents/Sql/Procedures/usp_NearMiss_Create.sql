-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NearMiss_Create.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [SafetyEvents].[NearMiss] row. Backs INearMissRepository.CreateAsync
--                  / NearMissDapperRepository.CreateAsync. Parameter set matches
--                  StoredProcedureBuilder.WithCanonicalHeaders plus the explicit Add* calls in
--                  NearMissDapperRepository.CreateAsync exactly. Status starts fixed at 'Reported'
--                  (see NearMiss.sql header -- no lifecycle-transition surface exists on this
--                  feature). TRY/CATCH with AuditDB/ErrorDB logging per the established pattern.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMiss_Create]
    @TenantId           UNIQUEIDENTIFIER,
    @CorrelationId      UNIQUEIDENTIFIER,
    @ActorId            UNIQUEIDENTIFIER,
    @NowUtc              DATETIME2(3),
    @NearMissId          UNIQUEIDENTIFIER,
    @SiteId              UNIQUEIDENTIFIER,
    @Title               NVARCHAR(200),
    @Description         NVARCHAR(4000),
    @OccurredAtUtc       DATETIME2(3),
    @PotentialSeverity   NVARCHAR(50),
    @HazardCategory      NVARCHAR(100),
    @ReportedByWorkerId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[NearMiss]
        (
            NearMissId, TenantId, SiteId, Title, Description, OccurredAtUtc, PotentialSeverity,
            HazardCategory, Status, ReportedByWorkerId,
            CreatedAtUtc, CreatedBy, LastModifiedByActorId, LastModifiedAtUtc
        )
        VALUES
        (
            @NearMissId, @TenantId, @SiteId, @Title, @Description, @OccurredAtUtc, @PotentialSeverity,
            @HazardCategory, N'Reported', @ReportedByWorkerId,
            @NowUtc, @ActorId, @ActorId, @NowUtc
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'NearMiss', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'NearMissId=', @NearMissId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'NearMissId=', @NearMissId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
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
