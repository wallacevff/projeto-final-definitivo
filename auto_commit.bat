@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM Verifica se está em um repositório Git
git rev-parse --is-inside-work-tree >nul 2>&1
IF ERRORLEVEL 1 (
    echo Você não está em um repositório Git.
    exit /b 1
)

REM Pega todos os arquivos modificados ou não rastreados
for /f "delims=" %%f in ('git status --porcelain -uall') do (
    set "linha=%%f"
    set "arquivo=!linha:~3!"

    REM Ignora entradas em branco
    if not "!arquivo!"=="" (
        echo Commitando: ♻️ TemplateSES - refactor(!arquivo!)
        git add "!arquivo!" >nul 2>&1
        git commit -m "♻️ TemplateSES - refactor(!arquivo!)"
    )
)

echo.
echo ✅ Todos os commits foram feitos com sucesso.
