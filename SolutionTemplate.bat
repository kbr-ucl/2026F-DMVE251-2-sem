@echo off
REM ============================================================================
REM  Clean Architecture — Projekt-skelet
REM  Kursus: 2026F-DMVE251-2-sem
REM
REM  Kør denne fil i en tom mappe. Den opretter:
REM    - Solution med 5 src-projekter og 2 testprojekter
REM    - Korrekte projekt-referencer (Dependency Rule)
REM    - NuGet-pakker (EF Core 10, Scalar, xunit.v3, Moq)
REM    - Mappestruktur i hvert projekt
REM    - Base classes (Entity, AggregateRoot)
REM ============================================================================

echo.
echo ============================================
echo  Clean Architecture — Projekt-skelet
echo ============================================
echo.

REM === Spørg om projektnavn ===
set /p PROJ_NAME="Indtast projektnavn (f.eks. MinKlinik): "

if "%PROJ_NAME%"=="" (
    echo FEJL: Projektnavn maa ikke vaere tomt.
    pause
    exit /b 1
)

REM Tjek for mellemrum
echo %PROJ_NAME%| findstr /C:" " >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo FEJL: Projektnavn maa ikke indeholde mellemrum.
    pause
    exit /b 1
)

echo.
echo Opretter projekt: %PROJ_NAME%
echo.

REM === Opret og gå ind i projektmappe ===
if exist %PROJ_NAME% (
    echo FEJL: Mappen %PROJ_NAME% eksisterer allerede.
    pause
    exit /b 1
)
mkdir %PROJ_NAME%
cd %PROJ_NAME%

REM === Solution ===
dotnet new sln -n %PROJ_NAME%
if %ERRORLEVEL% NEQ 0 (
    echo FEJL: Kunne ikke oprette solution. Er .NET 10 SDK installeret?
    pause
    exit /b 1
)

REM === Projekter ===
echo.
echo [1/7] Domain (classlib - ingen afhaengigheder)
dotnet new classlib -n %PROJ_NAME%.Domain -o src\%PROJ_NAME%.Domain -f net10.0
del src\%PROJ_NAME%.Domain\Class1.cs 2>nul

echo [2/7] Facade (classlib - ingen afhaengigheder)
dotnet new classlib -n %PROJ_NAME%.Facade -o src\%PROJ_NAME%.Facade -f net10.0
del src\%PROJ_NAME%.Facade\Class1.cs 2>nul

echo [3/7] UseCases (classlib - refererer Domain + Facade)
dotnet new classlib -n %PROJ_NAME%.UseCases -o src\%PROJ_NAME%.UseCases -f net10.0
del src\%PROJ_NAME%.UseCases\Class1.cs 2>nul

echo [4/7] Infrastructure (classlib - refererer Domain + Facade + UseCases)
dotnet new classlib -n %PROJ_NAME%.Infrastructure -o src\%PROJ_NAME%.Infrastructure -f net10.0
del src\%PROJ_NAME%.Infrastructure\Class1.cs 2>nul

echo [5/7] Api (webapi - refererer Facade + Infrastructure + UseCases)
dotnet new webapi -n %PROJ_NAME%.Api -o src\%PROJ_NAME%.Api -f net10.0 --use-controllers
del src\%PROJ_NAME%.Api\Controllers\WeatherForecastController.cs 2>nul
del src\%PROJ_NAME%.Api\WeatherForecast.cs 2>nul

echo [6/7] Domain.Tests (xunit.v3)
dotnet new xunit3 -n %PROJ_NAME%.Domain.Tests -o tests\%PROJ_NAME%.Domain.Tests -f net10.0
del tests\%PROJ_NAME%.Domain.Tests\UnitTest1.cs 2>nul

echo [7/7] UseCases.Tests (xunit.v3)
dotnet new xunit3 -n %PROJ_NAME%.UseCases.Tests -o tests\%PROJ_NAME%.UseCases.Tests -f net10.0
del tests\%PROJ_NAME%.UseCases.Tests\UnitTest1.cs 2>nul

REM === Tilføj projekter til solution ===
echo.
echo Tilfojer projekter til solution...
dotnet sln add src\%PROJ_NAME%.Domain\%PROJ_NAME%.Domain.csproj
dotnet sln add src\%PROJ_NAME%.Facade\%PROJ_NAME%.Facade.csproj
dotnet sln add src\%PROJ_NAME%.UseCases\%PROJ_NAME%.UseCases.csproj
dotnet sln add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj
dotnet sln add src\%PROJ_NAME%.Api\%PROJ_NAME%.Api.csproj
dotnet sln add tests\%PROJ_NAME%.Domain.Tests\%PROJ_NAME%.Domain.Tests.csproj
dotnet sln add tests\%PROJ_NAME%.UseCases.Tests\%PROJ_NAME%.UseCases.Tests.csproj

REM === Nullable + WarningsAsErrors i csproj ===
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Add-NullableProps.ps1"

REM === Projekt-referencer (Dependency Rule) ===
echo.
echo Opsaetter projekt-referencer (Dependency Rule)...

REM UseCases -> Domain + Facade
dotnet add src\%PROJ_NAME%.UseCases\%PROJ_NAME%.UseCases.csproj reference src\%PROJ_NAME%.Domain\%PROJ_NAME%.Domain.csproj
dotnet add src\%PROJ_NAME%.UseCases\%PROJ_NAME%.UseCases.csproj reference src\%PROJ_NAME%.Facade\%PROJ_NAME%.Facade.csproj

REM Infrastructure -> Domain + Facade + UseCases
dotnet add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj reference src\%PROJ_NAME%.Domain\%PROJ_NAME%.Domain.csproj
dotnet add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj reference src\%PROJ_NAME%.Facade\%PROJ_NAME%.Facade.csproj
dotnet add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj reference src\%PROJ_NAME%.UseCases\%PROJ_NAME%.UseCases.csproj

REM Api -> Facade + Infrastructure + UseCases
dotnet add src\%PROJ_NAME%.Api\%PROJ_NAME%.Api.csproj reference src\%PROJ_NAME%.Facade\%PROJ_NAME%.Facade.csproj
dotnet add src\%PROJ_NAME%.Api\%PROJ_NAME%.Api.csproj reference src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj
dotnet add src\%PROJ_NAME%.Api\%PROJ_NAME%.Api.csproj reference src\%PROJ_NAME%.UseCases\%PROJ_NAME%.UseCases.csproj

REM Domain.Tests -> Domain
dotnet add tests\%PROJ_NAME%.Domain.Tests\%PROJ_NAME%.Domain.Tests.csproj reference src\%PROJ_NAME%.Domain\%PROJ_NAME%.Domain.csproj

REM UseCases.Tests -> Domain + Facade + UseCases
dotnet add tests\%PROJ_NAME%.UseCases.Tests\%PROJ_NAME%.UseCases.Tests.csproj reference src\%PROJ_NAME%.Domain\%PROJ_NAME%.Domain.csproj
dotnet add tests\%PROJ_NAME%.UseCases.Tests\%PROJ_NAME%.UseCases.Tests.csproj reference src\%PROJ_NAME%.Facade\%PROJ_NAME%.Facade.csproj
dotnet add tests\%PROJ_NAME%.UseCases.Tests\%PROJ_NAME%.UseCases.Tests.csproj reference src\%PROJ_NAME%.UseCases\%PROJ_NAME%.UseCases.csproj

REM === NuGet-pakker ===
echo.
echo Installerer NuGet-pakker...

REM Infrastructure: EF Core 10
dotnet add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src\%PROJ_NAME%.Infrastructure\%PROJ_NAME%.Infrastructure.csproj package Microsoft.EntityFrameworkCore.InMemory

REM Api: OpenApi + Scalar
dotnet add src\%PROJ_NAME%.Api\%PROJ_NAME%.Api.csproj package Microsoft.AspNetCore.OpenApi
dotnet add src\%PROJ_NAME%.Api\%PROJ_NAME%.Api.csproj package Scalar.AspNetCore

REM Tests: xunit3-template giver allerede xunit.v3 — tilfoej kun Moq til UseCases.Tests
dotnet add tests\%PROJ_NAME%.UseCases.Tests\%PROJ_NAME%.UseCases.Tests.csproj package Moq

REM === Mappestruktur ===
echo.
echo Opretter mappestruktur...

REM Domain
mkdir src\%PROJ_NAME%.Domain\Entities 2>nul
mkdir src\%PROJ_NAME%.Domain\ValueObjects 2>nul
mkdir src\%PROJ_NAME%.Domain\Enums 2>nul
mkdir src\%PROJ_NAME%.Domain\Exceptions 2>nul

REM Facade
mkdir src\%PROJ_NAME%.Facade\UseCases 2>nul
mkdir src\%PROJ_NAME%.Facade\Queries 2>nul
mkdir src\%PROJ_NAME%.Facade\DTOs 2>nul

REM UseCases
REM (mapper oprettes efter behov for de enkelte use cases)

REM Infrastructure
mkdir src\%PROJ_NAME%.Infrastructure\Persistence 2>nul
mkdir src\%PROJ_NAME%.Infrastructure\Repositories 2>nul
mkdir src\%PROJ_NAME%.Infrastructure\QueryHandlers 2>nul

REM === Dummy.cs i tomme mapper ===
echo.
echo Tilfojer dummy.cs i tomme mapper...

REM Domain
(
echo namespace %PROJ_NAME%.Domain.Entities;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Domain\Entities\dummy.cs
(
echo namespace %PROJ_NAME%.Domain.ValueObjects;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Domain\ValueObjects\dummy.cs
(
echo namespace %PROJ_NAME%.Domain.Enums;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Domain\Enums\dummy.cs

REM Facade
(
echo namespace %PROJ_NAME%.Facade.UseCases;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Facade\UseCases\dummy.cs
(
echo namespace %PROJ_NAME%.Facade.Queries;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Facade\Queries\dummy.cs
(
echo namespace %PROJ_NAME%.Facade.DTOs;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Facade\DTOs\dummy.cs

REM Infrastructure
(
echo namespace %PROJ_NAME%.Infrastructure.Persistence;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Infrastructure\Persistence\dummy.cs
(
echo namespace %PROJ_NAME%.Infrastructure.Repositories;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Infrastructure\Repositories\dummy.cs
(
echo namespace %PROJ_NAME%.Infrastructure.QueryHandlers;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Infrastructure\QueryHandlers\dummy.cs

REM Api
(
echo namespace %PROJ_NAME%.Api.Controllers;
echo.
echo // Placeholder - erstattes med faktisk kode
) > src\%PROJ_NAME%.Api\Controllers\dummy.cs

REM === Starter-filer ===
echo.
echo Opretter starter-filer...

REM Entity + AggregateRoot base classes
(
echo namespace %PROJ_NAME%.Domain;
echo.
echo /// ^<summary^>
echo /// Base class for alle Entities. En Entity har en unik identitet ^(Id^).
echo /// To entities er ens hvis de har samme Id.
echo /// ^</summary^>
echo public abstract class Entity
echo {
echo     public Guid Id { get; protected set; }
echo }
echo.
echo /// ^<summary^>
echo /// Base class for Aggregate Roots.
echo ///
echo /// Saadan identificerer man en Aggregate Root:
echo ///   1. Egen livscyklus — kan oprettes/slettes uafhaengigt
echo ///   2. Transaktionsgraense — aendringer gemmes som en enhed
echo ///   3. Eget repository — hentes direkte fra databasen
echo ///   4. Refereres via ID — andre aggregater holder kun FK
echo /// ^</summary^>
echo public abstract class AggregateRoot : Entity
echo {
echo }
) > src\%PROJ_NAME%.Domain\AggregateRoot.cs

REM DomainException
(
echo namespace %PROJ_NAME%.Domain.Exceptions;
echo.
echo public class DomainException : Exception
echo {
echo     public DomainException^(string message^) : base^(message^) { }
echo }
) > src\%PROJ_NAME%.Domain\Exceptions\DomainException.cs

REM NotFoundException
(
echo namespace %PROJ_NAME%.Domain.Exceptions;
echo.
echo public class NotFoundException : Exception
echo {
echo     public NotFoundException^(string message^) : base^(message^) { }
echo }
) > src\%PROJ_NAME%.Domain\Exceptions\NotFoundException.cs

REM === Build test ===
echo.
echo Bygger solution...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ADVARSEL: Build fejlede. Tjek fejlmeddelelserne ovenfor.
) else (
    echo.
    echo Build OK!
)

REM === Opsummering ===
echo.
echo ============================================
echo  %PROJ_NAME% — Projekt-skelet oprettet!
echo ============================================
echo.
echo  Struktur:
echo    src\%PROJ_NAME%.Domain\          ^(ingen afhaengigheder^)
echo    src\%PROJ_NAME%.Facade\          ^(ingen afhaengigheder^)
echo    src\%PROJ_NAME%.UseCases\        ^(refererer Domain + Facade^)
echo    src\%PROJ_NAME%.Infrastructure\  ^(refererer Domain + Facade + UseCases^)
echo    src\%PROJ_NAME%.Api\             ^(refererer Facade + Infrastructure + UseCases^)
echo    tests\%PROJ_NAME%.Domain.Tests\  ^(refererer Domain^)
echo    tests\%PROJ_NAME%.UseCases.Tests\^(refererer Domain + Facade + UseCases^)
echo.
echo  NuGet-pakker:
echo    Infrastructure: EF Core 10 ^(SqlServer + InMemory^)
echo    Api:            Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore
echo    Tests:          xunit.v3 + Moq
echo.
echo  Starter-filer:
echo    Domain\AggregateRoot.cs        ^(Entity + AggregateRoot base classes^)
echo    Domain\Exceptions\             ^(DomainException + NotFoundException^)
echo.
echo  Naeste skridt:
echo    1. Aaben %PROJ_NAME%.sln i Visual Studio / Rider
echo    2. Implementer dine Value Objects i Domain\ValueObjects\
echo    3. Implementer dine Entities : AggregateRoot i Domain\Entities\
echo.
pause