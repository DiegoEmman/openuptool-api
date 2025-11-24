# openuptool-api

API para el proyecto OpenUpTool construida con .NET 10.0

## Estructura del Proyecto

```
OpenUpToolAPI/
├── Controllers/          # Controladores de API (endpoints)
├── Models/              # Modelos de dominio y entidades
├── Services/            # Lógica de negocio
├── Data/                # Capa de acceso a datos
│   └── Repositories/    # Implementaciones del patrón Repository
├── DTOs/                # Data Transfer Objects (Request/Response)
├── Middleware/          # Middleware personalizado
├── Extensions/          # Métodos de extensión y configuración
├── Configuration/       # Clases de configuración
├── Properties/          # Propiedades del proyecto
├── Program.cs           # Punto de entrada de la aplicación
├── appsettings.json     # Configuración de la aplicación
└── OpenUpToolAPI.csproj # Archivo del proyecto
```

## Requisitos

- .NET SDK 10.0 o superior

## Compilar el Proyecto

```bash
cd OpenUpToolAPI
dotnet build
```

## Ejecutar el Proyecto

```bash
cd OpenUpToolAPI
dotnet run
```

## Descripción de Carpetas

- **Controllers**: Contiene los controladores de API que definen los endpoints
- **Models**: Modelos de dominio que representan las entidades de negocio
- **Services**: Servicios que implementan la lógica de negocio
- **Data**: Capa de acceso a datos y contexto de base de datos
  - **Repositories**: Implementaciones del patrón Repository para acceso a datos
- **DTOs**: Data Transfer Objects para comunicación con la API
- **Middleware**: Componentes middleware personalizados
- **Extensions**: Métodos de extensión para mantener Program.cs limpio
- **Configuration**: Clases de configuración fuertemente tipadas