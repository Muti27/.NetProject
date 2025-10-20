@echo off

set ASPNETCORE_ENVIRONMENT=Production
dotnet ef database update --context PostgresDbContext --configuration Release

pause