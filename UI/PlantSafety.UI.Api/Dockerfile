# ============================================================================
#  Phase 3.8 (Deployment & Packaging): multi-stage Dockerfile for
#  PlantSafety.UI.Api. Build context is the repository root (no .sln
#  exists across this repo's 472 .csproj files, so the build stage copies
#  the full source tree rather than enumerating PlantSafety.UI.Api's
#  transitive ProjectReferences by hand) -- see .dockerignore for what's
#  excluded from that context (bin/obj/.git/.vs/.claude/TestResults).
#  No CI/CD pipeline exists anywhere in this sandbox to invoke this
#  automatically (flagged separately); this Dockerfile is a standalone,
#  manually invocable artifact: `docker build -f UI/PlantSafety.UI.Api/
#  Dockerfile -t plantsafety-ui-api .` from the repo root.
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish "UI/PlantSafety.UI.Api/PlantSafety.UI.Api.csproj" -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Runtime limits (Phase 2.5/3.5) are environment-manifest-driven, not baked into the
# image; Environment:Name defaults to Production here since that's this image's
# purpose -- override via -e Environment__Name=... for other environments.
ENV ASPNETCORE_URLS=http://+:8080
ENV Environment__Name=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "PlantSafety.UI.Api.dll"]
