@echo off
REM === 自動建立 PostgreSQL 專用 Migration ===
REM 取得使用者輸入的 Migration 名稱，如果沒輸入就用 InitPostgres

set /p MIGRATION_NAME=Migration名稱：
if "%MIGRATION_NAME%"=="" set MIGRATION_NAME=InitSqlite

set ASPNETCORE_ENVIRONMENT=Development
dotnet ef migrations add %MIGRATION_NAME% --context SQLiteDbContext --output-dir Migrations\SqliteMigrations

echo 建立完成
pause