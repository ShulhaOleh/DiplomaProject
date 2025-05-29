@echo off
setlocal enabledelayedexpansion

:: Файл для виводу
set OUTPUT=structure.txt
> %OUTPUT% echo Project structure:

:: Запуск рекурсії
call :Recurse "." ""
goto :EOF

:Recurse
set "current=%~1"
set "indent=%~2"

:: Проходимо по всіх директоріях
for /D %%D in ("%current%\*") do (
    set "folder=%%~nxD"
    if /I not "!folder!"=="bin" if /I not "!folder!"=="obj" if /I not "!folder!"==".git" (
        echo %indent%├── %%~nxD>> %OUTPUT%
        call :Recurse "%%~fD" "%indent%│   "
    )
)

:: Проходимо по всіх файлах (і виключаємо директорії)
for %%F in ("%current%\*") do (
    if not exist "%%F\" (
        set "file=%%~nxF"
        set "ext=%%~xF"
        if /I not "!ext!"==".dll" if /I not "!ext!"==".pdb" if /I not "!ext!"==".config" (
            echo %indent%│   %%~nxF>> %OUTPUT%
        )
    )
)

goto :EOF
