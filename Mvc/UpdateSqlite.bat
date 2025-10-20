@echo off

set ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --context SQLiteDbContext

pause