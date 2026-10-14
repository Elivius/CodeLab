@echo off
rem Creates App_Data\Database.mdf in LocalDB and runs Schema.sql and Seed.sql.
rem Requires sqlcmd on the PATH. To rebuild, delete Database.mdf and Database_log.ldf first.
setlocal

set "DATA_DIR=%~dp0"
set "SERVER=(LocalDB)\MSSQLLocalDB"
set "DB=CodeLabSetup"

if exist "%DATA_DIR%Database.mdf" goto :exists

sqlcmd -S "%SERVER%" -b -Q "CREATE DATABASE [%DB%] ON (NAME = CodeLab, FILENAME = '%DATA_DIR%Database.mdf') LOG ON (NAME = CodeLab_log, FILENAME = '%DATA_DIR%Database_log.ldf')"
if errorlevel 1 goto :fail

sqlcmd -S "%SERVER%" -b -d "%DB%" -i "%DATA_DIR%Schema.sql"
if errorlevel 1 goto :fail

sqlcmd -S "%SERVER%" -b -d "%DB%" -i "%DATA_DIR%Seed.sql"
if errorlevel 1 goto :fail

rem Detach so the site can attach the file through its own connection string
sqlcmd -S "%SERVER%" -b -Q "ALTER DATABASE [%DB%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; EXEC sp_detach_db '%DB%';"
if errorlevel 1 goto :fail

echo Database created: %DATA_DIR%Database.mdf
exit /b 0

:exists
echo Database.mdf already exists. Delete Database.mdf and Database_log.ldf to rebuild it.
exit /b 1

:fail
echo Database setup failed. See the error above.
exit /b 1
