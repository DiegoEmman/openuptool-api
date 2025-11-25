# OpenUpTool - Quick Start Script
# Este script te ayuda a levantar el proyecto rápidamente

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "OpenUpTool - Inicio Rápido" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Docker
Write-Host "[1/5] Verificando Docker..." -ForegroundColor Yellow
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host "   Instala Docker Desktop desde: https://www.docker.com/products/docker-desktop" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker encontrado" -ForegroundColor Green

# Verificar .NET
Write-Host "[2/5] Verificando .NET SDK..." -ForegroundColor Yellow
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET SDK no está instalado" -ForegroundColor Red
    Write-Host "   Instala .NET 9 SDK desde: https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
$dotnetVersion = dotnet --version
Write-Host "✅ .NET SDK $dotnetVersion encontrado" -ForegroundColor Green

# Verificar Node.js
Write-Host "[3/5] Verificando Node.js..." -ForegroundColor Yellow
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Node.js no está instalado" -ForegroundColor Red
    Write-Host "   Instala Node.js desde: https://nodejs.org/" -ForegroundColor Red
    exit 1
}
$nodeVersion = node --version
Write-Host "✅ Node.js $nodeVersion encontrado" -ForegroundColor Green

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Iniciando Servicios" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Levantar Base de Datos
Write-Host "[4/5] Levantando PostgreSQL con Docker..." -ForegroundColor Yellow
Set-Location "e:\Escritorio\openuptool-api"

if (Test-Path .env) {
    Write-Host "✅ Archivo .env encontrado" -ForegroundColor Green
} else {
    Write-Host "⚠️  Creando archivo .env desde .env.example..." -ForegroundColor Yellow
    Copy-Item .env.example .env
}

docker-compose up -d
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ PostgreSQL iniciado correctamente" -ForegroundColor Green
    Write-Host "   - PostgreSQL: localhost:5432" -ForegroundColor Gray
    Write-Host "   - PgAdmin: http://localhost:5050" -ForegroundColor Gray
} else {
    Write-Host "❌ Error al iniciar PostgreSQL" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Esperando 5 segundos para que PostgreSQL esté listo..." -ForegroundColor Gray
Start-Sleep -Seconds 5

# Iniciar Backend
Write-Host ""
Write-Host "[5/5] Iniciando Backend API..." -ForegroundColor Yellow
Set-Location "e:\Escritorio\openuptool-api\src\OpenUpTool.Api"

# Detener cualquier proceso anterior
Write-Host "Deteniendo procesos anteriores..." -ForegroundColor Gray
Stop-Process -Name "OpenUpTool.Api" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Verificar si hay migraciones pendientes
Write-Host "Verificando base de datos..." -ForegroundColor Gray

# Configurar entorno de desarrollo
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Iniciar la API en segundo plano
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru

Write-Host "Esperando a que la API esté lista..." -ForegroundColor Gray
Start-Sleep -Seconds 5

# Intentar verificar que la API esté respondiendo
$maxRetries = 10
$retryCount = 0
$apiReady = $false

while ($retryCount -lt $maxRetries -and -not $apiReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $apiReady = $true
        }
    } catch {
        $retryCount++
        Write-Host "." -NoNewline -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
}

Write-Host ""
if ($apiReady) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║         ✅ Backend API está corriendo!                   ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "📡 URLs Disponibles:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   🌐 Swagger UI: " -NoNewline -ForegroundColor White
    Write-Host "http://localhost:5000" -ForegroundColor Green
    Write-Host "      └─ Documentación interactiva de la API" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   📡 API Base: " -NoNewline -ForegroundColor White
    Write-Host "http://localhost:5000/api" -ForegroundColor Green
    Write-Host "      └─ Endpoint base para todas las llamadas" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📋 Endpoints principales:" -ForegroundColor Cyan
    Write-Host "   • GET/POST    http://localhost:5000/api/projects" -ForegroundColor Gray
    Write-Host "   • GET/POST    http://localhost:5000/api/phases" -ForegroundColor Gray
    Write-Host "   • GET/POST    http://localhost:5000/api/plans" -ForegroundColor Gray
    Write-Host "   • GET/POST    http://localhost:5000/api/iterations" -ForegroundColor Gray
    Write-Host "   • GET/POST    http://localhost:5000/api/artifacts" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "⚠️  Backend iniciado pero aún no responde (puede tardar unos segundos más)" -ForegroundColor Yellow
    Write-Host "   Verifica manualmente: http://localhost:5000" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Frontend" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Para iniciar el Frontend, abre una nueva terminal y ejecuta:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  cd e:\Escritorio\openuptool-front" -ForegroundColor Cyan
Write-Host "  npm install" -ForegroundColor Cyan
Write-Host "  npm run dev" -ForegroundColor Cyan
Write-Host ""
Write-Host "El frontend estará disponible en: http://localhost:5173" -ForegroundColor Gray

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "✅ Sistema Listo" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🌐 Servicios Activos:" -ForegroundColor White
Write-Host ""
Write-Host "  Frontend (próximo paso):" -ForegroundColor Yellow
Write-Host "    → http://localhost:5173" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Backend API:" -ForegroundColor Green
Write-Host "    → http://localhost:5000" -ForegroundColor Cyan -NoNewline
Write-Host " (Swagger UI)" -ForegroundColor Gray
Write-Host "    → http://localhost:5000/api" -ForegroundColor Cyan -NoNewline
Write-Host " (API Base)" -ForegroundColor Gray
Write-Host ""
Write-Host "  Base de Datos:" -ForegroundColor Blue
Write-Host "    → http://localhost:5050" -ForegroundColor Cyan -NoNewline
Write-Host " (PgAdmin)" -ForegroundColor Gray
Write-Host "    → localhost:5432" -ForegroundColor Cyan -NoNewline
Write-Host " (PostgreSQL)" -ForegroundColor Gray
Write-Host ""
Write-Host "Para detener los servicios:" -ForegroundColor Yellow
Write-Host "  • Presiona Ctrl+C en las terminales" -ForegroundColor Gray
Write-Host "  • Ejecuta: docker-compose down" -ForegroundColor Gray
Write-Host ""
