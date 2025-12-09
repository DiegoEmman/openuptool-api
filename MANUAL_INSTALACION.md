# 🚀 Manual de Instalación - OpenUpTool

## Tabla de Contenidos

1. [Requisitos del Sistema](#requisitos-del-sistema)
2. [Arquitectura de Despliegue](#arquitectura-de-despliegue)
3. [Instalación con Docker Compose (Recomendado)](#instalación-con-docker-compose-recomendado)
4. [Instalación Manual](#instalación-manual)
5. [Configuración](#configuración)
6. [Inicialización de Base de Datos](#inicialización-de-base-de-datos)
7. [Verificación de la Instalación](#verificación-de-la-instalación)
8. [Configuración de Producción](#configuración-de-producción)
9. [Actualización del Sistema](#actualización-del-sistema)
10. [Solución de Problemas](#solución-de-problemas)
11. [Respaldo y Recuperación](#respaldo-y-recuperación)

---

## Requisitos del Sistema

### Hardware Mínimo

- **CPU**: 2 cores (4 cores recomendado)
- **RAM**: 4 GB (8 GB recomendado)
- **Disco**: 20 GB de espacio libre (SSD recomendado)
- **Red**: Conexión a internet para descargar dependencias

### Software Requerido

#### Opción A: Con Docker (Recomendado)

- **Docker**: v20.10 o superior
- **Docker Compose**: v2.0 o superior
- **Sistema Operativo**: Windows 10/11, macOS 10.15+, Linux (Ubuntu 20.04+, CentOS 8+, etc.)

#### Opción B: Instalación Manual

**Backend (API)**:

- **.NET SDK**: 9.0 o superior
- **PostgreSQL**: 15 o 16
- **Git**: Para clonar el repositorio

**Frontend**:

- **Node.js**: 20.x LTS o superior
- **npm**: 10.x o superior

### Navegadores Soportados

- **Chrome**: 90+
- **Firefox**: 88+
- **Safari**: 14+
- **Edge**: 90+

---

## Arquitectura de Despliegue

OpenUpTool consta de tres componentes principales:

```
┌─────────────────────────────────────────────────────────┐
│                    Internet / Red Local                  │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │   Frontend (React)     │
        │   Puerto: 3000         │
        └────────┬───────────────┘
                 │
                 │ HTTP/HTTPS
                 ▼
        ┌────────────────────────┐
        │   API REST (.NET 9)    │
        │   Puerto: 5000/5001    │
        └────────┬───────────────┘
                 │
                 │ PostgreSQL Protocol
                 ▼
        ┌────────────────────────┐
        │   PostgreSQL 15/16     │
        │   Puerto: 5432         │
        └────────────────────────┘
```

**Componentes Adicionales**:

- **PgAdmin** (opcional): Interface web para gestionar PostgreSQL - Puerto 5050

---

## Instalación con Docker Compose (Recomendado)

Esta es la forma más rápida y sencilla de poner en marcha OpenUpTool.

### Paso 1: Clonar el Repositorio

```powershell
# Clonar ambos repositorios
git clone https://github.com/DiegoEmman/openuptool-api.git
git clone https://github.com/DiegoEmman/openuptool-front.git

# O si están en el mismo directorio padre
cd d:\Proyectos\OpenUpTool
```

### Paso 2: Configurar Variables de Entorno

#### Backend (openuptool-api)

1. Navega al directorio del backend:

```powershell
cd openuptool-api
```

2. Crea un archivo `.env` basado en el ejemplo:

```powershell
Copy-Item .env.example .env
```

3. Edita el archivo `.env` con tus valores:

```env
# Base de Datos
DB_HOST=postgres
DB_PORT=5432
DB_NAME=openuptool
DB_USER=openuptool_user
DB_PASSWORD=tu_password_seguro_aqui

# JWT Configuration
JWT_SECRET=tu_secreto_jwt_muy_largo_y_seguro_min_256_bits
JWT_ISSUER=OpenUpTool
JWT_AUDIENCE=OpenUpTool-Users
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_EXPIRATION_DAYS=7

# CORS
CORS_ORIGINS=http://localhost:3000,http://localhost:5173

# Storage
STORAGE_TYPE=FileSystem
STORAGE_PATH=./uploads
MAX_FILE_SIZE_MB=50

# SMTP (Opcional - para notificaciones por email)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=tu_email@gmail.com
SMTP_PASSWORD=tu_password_smtp
SMTP_FROM=noreply@openuptool.com
SMTP_FROM_NAME=OpenUpTool

# Features
ENABLE_SWAGGER=true
ENABLE_AUDIT_LOG=true
ENABLE_RATE_LIMITING=false

# PgAdmin
PGADMIN_EMAIL=admin@openuptool.com
PGADMIN_PASSWORD=admin
PGADMIN_PORT=5050
```

**⚠️ Importante**:

- Cambia `DB_PASSWORD` por una contraseña segura
- Cambia `JWT_SECRET` por una cadena aleatoria de al menos 256 bits (32 caracteres)
- Configura SMTP solo si deseas enviar emails

#### Frontend (openuptool-front)

1. Navega al directorio del frontend:

```powershell
cd ..\openuptool-front
```

2. Crea un archivo `.env` (si no existe):

```powershell
New-Item -Path .env -ItemType File
```

3. Añade la URL de la API:

```env
VITE_API_URL=http://localhost:5000/api
```

### Paso 3: Levantar los Servicios

#### Backend con Docker Compose

1. En el directorio `openuptool-api`:

```powershell
cd ..\openuptool-api
docker-compose up -d
```

Esto iniciará:

- PostgreSQL en el puerto 5432
- PgAdmin en el puerto 5050

2. Verificar que los contenedores estén corriendo:

```powershell
docker-compose ps
```

Deberías ver:

```
NAME                   STATUS          PORTS
openuptool-db          Up              0.0.0.0:5432->5432/tcp
openuptool-pgadmin     Up              0.0.0.0:5050->80/tcp
```

#### Iniciar la API

```powershell
# Instalar dependencias (primera vez)
dotnet restore

# Aplicar migraciones de base de datos
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api

# Iniciar la API
dotnet run --project src/OpenUpTool.Api
```

La API estará disponible en:

- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: http://localhost:5000/swagger

#### Iniciar el Frontend

1. En el directorio `openuptool-front`:

```powershell
cd ..\openuptool-front

# Instalar dependencias (primera vez)
npm install

# Iniciar en modo desarrollo
npm run dev
```

El frontend estará disponible en: http://localhost:5173

---

## Instalación Manual

Si prefieres no usar Docker, puedes instalar cada componente manualmente.

### Paso 1: Instalar PostgreSQL

#### En Windows

1. Descarga PostgreSQL 15 o 16 desde: https://www.postgresql.org/download/windows/
2. Ejecuta el instalador
3. Durante la instalación:

   - Establece una contraseña para el usuario `postgres`
   - Selecciona el puerto 5432 (por defecto)
   - Instala pgAdmin 4 (opcional)

4. Crea la base de datos:

```powershell
# Conectarse a PostgreSQL
psql -U postgres

# Crear usuario y base de datos
CREATE USER openuptool_user WITH PASSWORD 'tu_password_seguro';
CREATE DATABASE openuptool OWNER openuptool_user;
GRANT ALL PRIVILEGES ON DATABASE openuptool TO openuptool_user;
\q
```

#### En Linux (Ubuntu/Debian)

```bash
# Instalar PostgreSQL
sudo apt update
sudo apt install postgresql postgresql-contrib

# Crear usuario y base de datos
sudo -u postgres psql

CREATE USER openuptool_user WITH PASSWORD 'tu_password_seguro';
CREATE DATABASE openuptool OWNER openuptool_user;
GRANT ALL PRIVILEGES ON DATABASE openuptool TO openuptool_user;
\q
```

#### En macOS

```bash
# Con Homebrew
brew install postgresql@15
brew services start postgresql@15

# Crear usuario y base de datos
psql postgres

CREATE USER openuptool_user WITH PASSWORD 'tu_password_seguro';
CREATE DATABASE openuptool OWNER openuptool_user;
GRANT ALL PRIVILEGES ON DATABASE openuptool TO openuptool_user;
\q
```

### Paso 2: Instalar .NET SDK 9.0

#### Windows

1. Descarga desde: https://dotnet.microsoft.com/download/dotnet/9.0
2. Ejecuta el instalador
3. Verifica la instalación:

```powershell
dotnet --version
```

#### Linux

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
```

#### macOS

```bash
brew install dotnet@9
```

### Paso 3: Instalar Node.js 20 LTS

#### Windows

1. Descarga desde: https://nodejs.org/
2. Ejecuta el instalador
3. Verifica:

```powershell
node --version
npm --version
```

#### Linux

```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs
```

#### macOS

```bash
brew install node@20
```

### Paso 4: Configurar el Backend

1. Clona el repositorio:

```powershell
git clone https://github.com/DiegoEmman/openuptool-api.git
cd openuptool-api
```

2. Crea el archivo `.env` siguiendo las instrucciones del Paso 2 de Docker Compose

3. Actualiza `appsettings.json` si es necesario:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=openuptool;Username=openuptool_user;Password=tu_password"
  }
}
```

4. Restaura los paquetes:

```powershell
dotnet restore
```

5. Aplica las migraciones:

```powershell
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

6. Inicia la API:

```powershell
dotnet run --project src/OpenUpTool.Api
```

### Paso 5: Configurar el Frontend

1. Clona el repositorio:

```powershell
git clone https://github.com/DiegoEmman/openuptool-front.git
cd openuptool-front
```

2. Instala dependencias:

```powershell
npm install
```

3. Configura el archivo `.env`:

```env
VITE_API_URL=http://localhost:5000/api
```

4. Inicia el servidor de desarrollo:

```powershell
npm run dev
```

---

## Configuración

### Configuración de la Base de Datos

El archivo `docker/init-scripts/` contiene scripts de inicialización opcionales para:

- Crear extensiones de PostgreSQL
- Insertar datos de prueba
- Configurar roles y permisos

### Configuración de CORS

Para permitir acceso desde otros dominios, actualiza `CORS_ORIGINS` en el archivo `.env`:

```env
CORS_ORIGINS=http://localhost:3000,http://localhost:5173,https://midominio.com
```

### Configuración de Almacenamiento de Archivos

Por defecto, los archivos se guardan en el sistema de archivos local:

```env
STORAGE_TYPE=FileSystem
STORAGE_PATH=./uploads
MAX_FILE_SIZE_MB=50
```

Para usar otro sistema de almacenamiento (Azure Blob, AWS S3), modifica estas variables y actualiza el código correspondiente.

### Configuración de SMTP

Para habilitar notificaciones por email:

1. Obtén credenciales de un servidor SMTP (Gmail, SendGrid, etc.)
2. Actualiza las variables SMTP en `.env`
3. Si usas Gmail, habilita "Acceso de aplicaciones menos seguras" o genera una contraseña de aplicación

### Configuración de JWT

El token JWT es usado para autenticación:

```env
JWT_SECRET=cadena_aleatoria_minimo_256_bits
JWT_EXPIRATION_MINUTES=60
```

**Generar un secreto seguro**:

```powershell
# PowerShell
$bytes = New-Object byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

---

## Inicialización de Base de Datos

### Migraciones de Entity Framework Core

Las migraciones gestionan el esquema de la base de datos.

#### Aplicar Migraciones Existentes

```powershell
cd openuptool-api
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

#### Crear una Nueva Migración

```powershell
dotnet ef migrations add NombreDeLaMigracion --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

#### Revertir una Migración

```powershell
dotnet ef database update MigracionAnterior --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

### Datos de Prueba

El sistema incluye scripts SQL para insertar datos de prueba:

```powershell
# Acceder a PostgreSQL
psql -U openuptool_user -d openuptool -h localhost

# Ejecutar script
\i docker/init-scripts/01-seed-data.sql
```

O con Docker:

```powershell
docker exec -i openuptool-db psql -U openuptool_user -d openuptool < docker/init-scripts/01-seed-data.sql
```

### Usuarios por Defecto

El sistema crea usuarios de prueba con las siguientes credenciales (ver `USUARIOS_PRUEBA.txt`):

- **Admin**: admin@openuptool.com / Admin123!
- **Manager**: manager@openuptool.com / Manager123!
- **Developer**: developer@openuptool.com / Developer123!
- **Tester**: tester@openuptool.com / Tester123!

⚠️ **Importante**: Cambia estas contraseñas inmediatamente en producción.

---

## Verificación de la Instalación

### 1. Verificar PostgreSQL

```powershell
# Con Docker
docker exec -it openuptool-db psql -U openuptool_user -d openuptool -c "SELECT version();"

# Sin Docker
psql -U openuptool_user -d openuptool -c "SELECT version();"
```

### 2. Verificar la API

```powershell
# Verificar que la API responde
curl http://localhost:5000/health

# O abre en el navegador
start http://localhost:5000/swagger
```

### 3. Verificar el Frontend

Abre en tu navegador: http://localhost:5173

Deberías ver la página de login de OpenUpTool.

### 4. Prueba de Login

1. Accede a http://localhost:5173
2. Usa las credenciales de prueba:
   - Email: `admin@openuptool.com`
   - Password: `Admin123!`
3. Deberías acceder al dashboard principal

### 5. Verificar Conectividad API-Frontend

1. Abre las herramientas de desarrollador del navegador (F12)
2. Ve a la pestaña "Network"
3. Intenta hacer login
4. Deberías ver una petición a `http://localhost:5000/api/auth/login` con respuesta 200 OK

---

## Configuración de Producción

### Preparar el Backend para Producción

1. **Compilar en modo Release**:

```powershell
dotnet publish src/OpenUpTool.Api/OpenUpTool.Api.csproj -c Release -o ./publish
```

2. **Actualizar `appsettings.Production.json`**:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "ExpirationMinutes": 30
  },
  "Features": {
    "EnableSwagger": false
  }
}
```

3. **Configurar HTTPS**:

- Obtén un certificado SSL (Let's Encrypt, certificado comprado, etc.)
- Configura Kestrel en `appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      },
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/path/to/certificate.pfx",
          "Password": "certificate_password"
        }
      }
    }
  }
}
```

### Preparar el Frontend para Producción

1. **Construir para producción**:

```powershell
npm run build
```

Esto genera la carpeta `build/` con los archivos optimizados.

2. **Actualizar la URL de la API**:

Edita `.env.production`:

```env
VITE_API_URL=https://api.midominio.com/api
```

3. **Servir los archivos estáticos**:

Puedes usar:

- **Nginx**
- **Apache**
- **IIS** (Windows)
- **Servicio cloud** (Azure Static Web Apps, AWS S3 + CloudFront, etc.)

### Configurar Nginx como Proxy Reverso

Ejemplo de configuración Nginx:

```nginx
# Frontend
server {
    listen 80;
    server_name openuptool.midominio.com;
    root /var/www/openuptool-front/build/client;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}

# API
server {
    listen 80;
    server_name api.openuptool.midominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Desplegar con Docker en Producción

1. **Construir las imágenes**:

Backend:

```powershell
cd openuptool-api
docker build -t openuptool-api:latest .
```

Frontend:

```powershell
cd openuptool-front
docker build -t openuptool-front:latest .
```

2. **Crear `docker-compose.prod.yml`**:

```yaml
version: "3.8"

services:
  postgres:
    image: postgres:15
    container_name: openuptool-db-prod
    restart: always
    environment:
      POSTGRES_DB: ${DB_NAME}
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data_prod:/var/lib/postgresql/data
    networks:
      - openuptool-network-prod

  api:
    image: openuptool-api:latest
    container_name: openuptool-api-prod
    restart: always
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    env_file:
      - .env
    depends_on:
      - postgres
    networks:
      - openuptool-network-prod

  frontend:
    image: openuptool-front:latest
    container_name: openuptool-front-prod
    restart: always
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=production
    networks:
      - openuptool-network-prod

volumes:
  postgres_data_prod:

networks:
  openuptool-network-prod:
    driver: bridge
```

3. **Iniciar en producción**:

```powershell
docker-compose -f docker-compose.prod.yml up -d
```

### Consideraciones de Seguridad en Producción

1. ✅ **Cambia todas las contraseñas por defecto**
2. ✅ **Usa HTTPS exclusivamente**
3. ✅ **Configura firewall para limitar acceso a PostgreSQL**
4. ✅ **Establece límites de rate limiting**
5. ✅ **Configura logs y monitoreo**
6. ✅ **Realiza backups automáticos**
7. ✅ **Deshabilita Swagger en producción** (`ENABLE_SWAGGER=false`)
8. ✅ **Configura CORS solo para dominios permitidos**
9. ✅ **Actualiza regularmente las dependencias**
10. ✅ **Implementa escaneo de vulnerabilidades**

---

## Actualización del Sistema

### Actualizar el Backend

1. **Hacer backup de la base de datos** (ver sección de Respaldo)

2. **Descargar la nueva versión**:

```powershell
cd openuptool-api
git pull origin main
```

3. **Restaurar dependencias**:

```powershell
dotnet restore
```

4. **Aplicar nuevas migraciones**:

```powershell
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

5. **Reiniciar la API**:

```powershell
# Si está corriendo como servicio
systemctl restart openuptool-api

# O con Docker
docker-compose restart api
```

### Actualizar el Frontend

1. **Descargar la nueva versión**:

```powershell
cd openuptool-front
git pull origin main
```

2. **Instalar nuevas dependencias**:

```powershell
npm install
```

3. **Reconstruir**:

```powershell
npm run build
```

4. **Reiniciar el servidor**:

```powershell
npm run start
```

---

## Solución de Problemas

### La API no inicia

**Error**: `Unable to connect to the database`

**Solución**:

1. Verifica que PostgreSQL esté corriendo:

```powershell
docker ps | Select-String postgres
```

2. Verifica las credenciales en `.env`
3. Prueba la conexión manualmente:

```powershell
psql -U openuptool_user -d openuptool -h localhost
```

### El Frontend no se conecta a la API

**Error**: `Network Error` o `CORS Error`

**Solución**:

1. Verifica que la API esté corriendo: `curl http://localhost:5000/health`
2. Verifica la URL de la API en `.env` del frontend
3. Asegúrate de que CORS esté configurado correctamente en el backend
4. Revisa la consola del navegador (F12) para más detalles

### Error de Migraciones

**Error**: `The database already exists`

**Solución**:

```powershell
# Eliminar la base de datos y recrearla
dotnet ef database drop --force --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

### Puerto ya en uso

**Error**: `Address already in use`

**Solución**:

Windows:

```powershell
# Ver qué proceso usa el puerto 5000
Get-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess

# Detener el proceso
Stop-Process -Id <PID>
```

Linux/macOS:

```bash
# Ver qué proceso usa el puerto
lsof -i :5000

# Detener el proceso
kill -9 <PID>
```

### Docker: Contenedor no inicia

**Solución**:

```powershell
# Ver logs del contenedor
docker logs openuptool-db

# Reiniciar contenedores
docker-compose down
docker-compose up -d

# Eliminar volúmenes y reiniciar
docker-compose down -v
docker-compose up -d
```

### Error de Autenticación JWT

**Error**: `Invalid token` o `401 Unauthorized`

**Solución**:

1. Verifica que `JWT_SECRET` esté configurado en `.env`
2. Asegúrate de que sea el mismo secreto en todos los entornos
3. Cierra sesión y vuelve a iniciar sesión
4. Verifica que el token no haya expirado

---

## Respaldo y Recuperación

### Backup de la Base de Datos

#### Con Docker

```powershell
# Crear backup
docker exec openuptool-db pg_dump -U openuptool_user openuptool > backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql

# Restaurar backup
Get-Content backup_20241209_120000.sql | docker exec -i openuptool-db psql -U openuptool_user -d openuptool
```

#### Sin Docker

```powershell
# Crear backup
pg_dump -U openuptool_user -h localhost openuptool > backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql

# Restaurar backup
psql -U openuptool_user -h localhost -d openuptool < backup_20241209_120000.sql
```

### Backup de Archivos

Los archivos subidos se encuentran en la carpeta configurada en `STORAGE_PATH` (por defecto `./uploads`).

```powershell
# Crear backup de archivos
Compress-Archive -Path ./uploads -DestinationPath "backup_files_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
```

### Automatizar Backups

#### Windows (PowerShell Script)

Crea `backup.ps1`:

```powershell
$BackupDir = "C:\Backups\OpenUpTool"
$Date = Get-Date -Format 'yyyyMMdd_HHmmss'

# Crear directorio si no existe
New-Item -ItemType Directory -Force -Path $BackupDir

# Backup de base de datos
docker exec openuptool-db pg_dump -U openuptool_user openuptool > "$BackupDir\db_$Date.sql"

# Backup de archivos
Compress-Archive -Path .\uploads -DestinationPath "$BackupDir\files_$Date.zip"

# Eliminar backups antiguos (más de 30 días)
Get-ChildItem -Path $BackupDir -Recurse -File | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item
```

Programa con Tarea Programada:

```powershell
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Path\To\backup.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "OpenUpTool-Backup" -Description "Backup diario de OpenUpTool"
```

#### Linux (Cron)

```bash
# Editar crontab
crontab -e

# Añadir línea para backup diario a las 2 AM
0 2 * * * /usr/local/bin/openuptool-backup.sh
```

Crear `/usr/local/bin/openuptool-backup.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/backups/openuptool"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

# Backup de base de datos
docker exec openuptool-db pg_dump -U openuptool_user openuptool > $BACKUP_DIR/db_$DATE.sql

# Backup de archivos
tar -czf $BACKUP_DIR/files_$DATE.tar.gz /path/to/uploads

# Eliminar backups antiguos (más de 30 días)
find $BACKUP_DIR -mtime +30 -delete
```

---

## Monitoreo y Logs

### Logs de la API

```powershell
# Ver logs en tiempo real
dotnet run --project src/OpenUpTool.Api

# O con Docker
docker logs -f openuptool-api
```

Los logs se guardan en `logs/` por defecto.

### Logs de PostgreSQL

```powershell
# Con Docker
docker logs openuptool-db

# Ver logs de consultas lentas
docker exec -it openuptool-db psql -U openuptool_user -d openuptool -c "SELECT * FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10;"
```

### Métricas de Rendimiento

Considera implementar:

- **Application Insights** (Azure)
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Prometheus + Grafana**

---

## Soporte

Para más ayuda:

- 📚 **Documentación de Arquitectura**: Ver `ARCHITECTURE.md`
- 📖 **Manual de Usuario**: Ver `MANUAL_USUARIO.md`
- 🐛 **Reportar Issues**: GitHub Issues
- 💬 **Comunidad**: [Enlace al foro o chat]

---

**OpenUpTool v1.0**  
© 2024 - Instalación y Configuración
