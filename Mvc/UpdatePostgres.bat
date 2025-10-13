@echo off

dotnet ef database update --context PostgresDbContext --configuration Release

pause