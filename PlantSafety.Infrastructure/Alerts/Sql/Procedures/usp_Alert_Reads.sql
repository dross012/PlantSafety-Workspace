-- ============================================================================
--  Module:  Alerts.Alert | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Read-only stored procedures for the Alert aggregate: aggregate
--               rehydration reads (Domain/Application command repositories) and
--               read-model DTO reads (Application query repository).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Single aggregate row, for rehydration (no TenantId filter -- the Domain
-- repository interface intentionally carries no tenant parameter; see
-- AlertDapperRepository remarks).
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetById]
    @AlertId  UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
           [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude], [LocationIndoorPositionReference],
           [TriggeredAtUtc], [AcknowledgedAtUtc], [ResolvedAtUtc], [ClosedAtUtc], [CloseOutVerification],
           [CreatedAtUtc], [CreatedBy]
    FROM [Alerts].[Alert]
    WHERE [AlertId] = @AlertId AND ISNULL([IsDeleted], 0) = 0;
END
GO

-- All non-deleted aggregate rows for in-memory AlertSpecification filtering
-- (Domain IAlertRepository.ListAsync has no tenant parameter to scope by).
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_ListAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
           [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude], [LocationIndoorPositionReference],
           [TriggeredAtUtc], [AcknowledgedAtUtc], [ResolvedAtUtc], [ClosedAtUtc], [CloseOutVerification],
           [CreatedAtUtc], [CreatedBy]
    FROM [Alerts].[Alert]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetDeliveryRecords]
    @AlertId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DeliveryRecordId], [AlertId], [Channel], [Succeeded], [AttemptedAtUtc], [FailureReason]
    FROM [Alerts].[AlertDeliveryRecord]
    WHERE [AlertId] = @AlertId AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [AttemptedAtUtc] ASC;
END
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetResponseActions]
    @AlertId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ResponseActionId], [AlertId], [ActionDescription], [TakenAtUtc], [TakenByActorKind], [TakenByUserId], [Notes]
    FROM [Alerts].[AlertResponseAction]
    WHERE [AlertId] = @AlertId AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [TakenAtUtc] ASC;
END
GO

-- Read-model DTO: single Alert by id (tenant-scoped).
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetByIdDto]
    @TenantId UNIQUEIDENTIFIER,
    @AlertId  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
           [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude],
           [TriggeredAtUtc], [AcknowledgedAtUtc], [ResolvedAtUtc], [ClosedAtUtc], [CloseOutVerification]
    FROM [Alerts].[Alert]
    WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
END
GO

-- Read-model DTO: all active (not-Closed) Alerts, optionally site-scoped.
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetActive]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
           [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude],
           [TriggeredAtUtc], [AcknowledgedAtUtc], [ResolvedAtUtc], [ClosedAtUtc], [CloseOutVerification]
    FROM [Alerts].[Alert]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] <> N'Closed'
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [TriggeredAtUtc] DESC;
END
GO

-- Read-model DTO: Alerts filtered by catalog category, optionally site-scoped.
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetByCategory]
    @TenantId     UNIQUEIDENTIFIER,
    @SiteId       UNIQUEIDENTIFIER = NULL,
    @CategoryCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
           [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude],
           [TriggeredAtUtc], [AcknowledgedAtUtc], [ResolvedAtUtc], [ClosedAtUtc], [CloseOutVerification]
    FROM [Alerts].[Alert]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [CategoryCode] = @CategoryCode
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [TriggeredAtUtc] DESC;
END
GO

-- Read-model DTO: total row count for a paginated Alert history query. Split from the page
-- query below (rather than a single multi-result-set procedure) because the shared
-- DapperQueryExecutor exposes single-result-set helpers only (QueryAsync/QuerySingleOrDefaultAsync),
-- not Dapper's QueryMultiple.
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetHistoryCount]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @FromUtc     DATETIME2(3),
    @ToUtc       DATETIME2(3),
    @PageNumber  INT = 1,
    @PageSize    INT = 25
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM [Alerts].[Alert]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [TriggeredAtUtc] >= @FromUtc AND [TriggeredAtUtc] <= @ToUtc
      AND ISNULL([IsDeleted], 0) = 0;
END
GO

-- Read-model DTO: deterministically ordered, paginated Alert history (all statuses).
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_GetHistoryPage]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @FromUtc     DATETIME2(3),
    @ToUtc       DATETIME2(3),
    @PageNumber  INT,
    @PageSize    INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
           [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude],
           [TriggeredAtUtc], [AcknowledgedAtUtc], [ResolvedAtUtc], [ClosedAtUtc], [CloseOutVerification]
    FROM [Alerts].[Alert]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [TriggeredAtUtc] >= @FromUtc AND [TriggeredAtUtc] <= @ToUtc
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [TriggeredAtUtc] DESC, [AlertId] ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
