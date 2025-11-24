# GUÍA DE INICIO RÁPIDO - OpenUpTool API

## 🚀 Primeros Pasos

### 1. Requisitos Previos

Asegúrate de tener instalado:

- ✅ [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop)
- ✅ [Git](https://git-scm.com/)
- ✅ Editor: Visual Studio 2022, VS Code o Rider

### 2. Instalación

```bash
# Clonar repositorio
git clone https://github.com/DiegoEmman/openuptool-api.git
cd openuptool-api

# Configurar variables de entorno
Copy-Item .env.example .env

# Iniciar base de datos
docker-compose up -d

# Restaurar dependencias
dotnet restore

# Compilar proyecto
dotnet build
```

### 3. Verificar Instalación

```bash
# Verificar que Docker está corriendo
docker-compose ps

# Deberías ver:
# openuptool-db      running   0.0.0.0:5432->5432/tcp
# openuptool-pgadmin running   0.0.0.0:5050->80/tcp
```

### 4. Ejecutar API

```bash
# Desde la raíz del proyecto
dotnet run --project src/OpenUpTool.Api/OpenUpTool.Api.csproj

# O con hot reload
dotnet watch --project src/OpenUpTool.Api/OpenUpTool.Api.csproj
```

### 5. Verificar que Funciona

Abre tu navegador en:

- 🌐 API: http://localhost:5000
- 📚 Swagger: http://localhost:5000/swagger
- 🗄️ PgAdmin: http://localhost:5050

---

## 🎯 Próximos Pasos

1. ✅ **Familiarízate con la estructura**: Lee [ARCHITECTURE.md](./ARCHITECTURE.md)
2. ✅ **Revisa las HUs**: Están en el README principal
3. ✅ **Configura tu IDE**: Extensions recomendadas abajo
4. ✅ **Ejecuta los tests**: `dotnet test`

---

## 🛠️ Extensiones Recomendadas (VS Code)

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "ms-azuretools.vscode-docker",
    "humao.rest-client",
    "patcx.vscode-nuget-gallery"
  ]
}
```

---

## 📚 Comandos Más Usados

```bash
# Desarrollo
dotnet run --project src/OpenUpTool.Api/OpenUpTool.Api.csproj
dotnet watch --project src/OpenUpTool.Api/OpenUpTool.Api.csproj
dotnet test

# Docker
docker-compose up -d
docker-compose down
docker-compose logs -f

# EF Core (cuando estén las migraciones)
dotnet ef migrations add InitialCreate --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

---

## ❓ Problemas Comunes

### El puerto 5000 está ocupado

```bash
# Cambiar en .env
API_PORT=5001
```

### Docker no inicia

```bash
# En Windows, asegúrate de que Docker Desktop está ejecutándose
# Luego:
docker-compose down -v
docker-compose up -d
```

### Error de restauración de paquetes

```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📖 Documentación Adicional

- [README.md](./README.md) - Documentación completa
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Arquitectura del proyecto
- [Historias de Usuario](./README.md#-historias-de-usuario) - Product Backlog

---

## 💡 Tips

1. **Usa `dotnet watch`** para hot reload durante desarrollo
2. **PgAdmin** está disponible en http://localhost:5050 para explorar la BD
3. **Swagger** es tu mejor amigo para probar endpoints
4. **Revisa los logs** con `docker-compose logs -f` si hay problemas con BD

---

¡Listo para desarrollar! 🎉
