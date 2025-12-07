using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenUpTool.Api.Middleware;
using OpenUpTool.Core.Interfaces;
using OpenUpTool.Core.Services;
using OpenUpTool.Infrastructure.Data;
using OpenUpTool.Infrastructure.Repositories;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenUpTool.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        // Cargar variables de entorno desde .env
        // Buscar el archivo .env en múltiples ubicaciones posibles
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"), // Desde raíz del proyecto
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"), // Desde src/OpenUpTool.Api
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", ".env"), // Desde bin/Debug/net9.0
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env"), // Alternativa desde bin
        };

        string? foundEnvPath = null;
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                foundEnvPath = fullPath;
                break;
            }
        }
        
        if (foundEnvPath != null)
        {
            Env.Load(foundEnvPath);
            Console.WriteLine($"✓ Archivo .env cargado desde: {foundEnvPath}");
        }
        else
        {
            Console.WriteLine($"⚠ Archivo .env no encontrado. Usando valores por defecto.");
            Console.WriteLine($"   Directorio actual: {Directory.GetCurrentDirectory()}");
        }
        
        // Asegurar que el entorno esté en Development si ASPNETCORE_ENVIRONMENT está configurado
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(aspnetEnv))
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", aspnetEnv);
        }

        var builder = WebApplication.CreateBuilder(args);

        // Configurar logging - TEMPORAL: nivel Information para debug
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug); // Cambiado a Debug para ver más detalles
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning); // Ver warnings de EF
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

        // Configurar conexión a base de datos
        // Construir la cadena de conexión desde variables de entorno
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "openuptool";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "openuptool_user";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "DevPassword123!";
        
        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
        
        // Configurar AppContext para deshabilitar timestamps con zona horaria
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        builder.Services.AddDbContext<OpenUpToolDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Configurar CORS
        var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:5173,http://localhost:3000";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(corsOrigins.Split(','))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Registrar repositorios
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IPhaseRepository, PhaseRepository>();
        builder.Services.AddScoped<IProjectPlanRepository, ProjectPlanRepository>();
        builder.Services.AddScoped<IIterationRepository, IterationRepository>();
        builder.Services.AddScoped<IIterationTaskRepository, IterationTaskRepository>();
        builder.Services.AddScoped<IIterationProgressRepository, IterationProgressRepository>();
        builder.Services.AddScoped<IArtifactRepository, ArtifactRepository>();
        builder.Services.AddScoped<IArtifactTypeRepository, ArtifactTypeRepository>();
        builder.Services.AddScoped<IArtifactVersionRepository, ArtifactVersionRepository>();
        builder.Services.AddScoped<ITestExecutionRepository, TestExecutionRepository>();
        builder.Services.AddScoped<IDefectRepository, DefectRepository>();
        builder.Services.AddScoped<IUserStoryRepository, UserStoryRepository>();
        builder.Services.AddScoped<IIterationScopeRepository, IterationScopeRepository>();
        builder.Services.AddScoped<IProjectInvitationRepository, ProjectInvitationRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IProjectUserRoleRepository, ProjectUserRoleRepository>();
        builder.Services.AddScoped<IMicroincrementRepository, MicroincrementRepository>();
        builder.Services.AddScoped<IProjectClosureRepository, ProjectClosureRepository>();
        builder.Services.AddScoped<IFinalBuildRepository, FinalBuildRepository>();
        builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        builder.Services.AddScoped<IWorkflowStateRepository, WorkflowStateRepository>();
        builder.Services.AddScoped<IWorkflowStateResponsibleRepository, WorkflowStateResponsibleRepository>();
        builder.Services.AddScoped<IArtifactStateHistoryRepository, ArtifactStateHistoryRepository>();
        builder.Services.AddScoped<IWorkflowPermissionRepository, WorkflowPermissionRepository>();

        // Registrar servicios
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IPhaseService, PhaseService>();
        builder.Services.AddScoped<IProjectPlanService, ProjectPlanService>();
        builder.Services.AddScoped<IIterationService, IterationService>();
        builder.Services.AddScoped<IIterationTaskService, IterationTaskService>();
        builder.Services.AddScoped<IIterationProgressService, IterationProgressService>();
        builder.Services.AddScoped<IArtifactService, ArtifactService>();
        builder.Services.AddScoped<IArtifactTypeService, ArtifactTypeService>();
        builder.Services.AddScoped<IArtifactVersionService, ArtifactVersionService>();
        builder.Services.AddScoped<ITestExecutionService, TestExecutionService>();
        builder.Services.AddScoped<IDefectService, DefectService>();
        builder.Services.AddScoped<IUserStoryService, UserStoryService>();
        builder.Services.AddScoped<IIterationScopeService, IterationScopeService>();
        builder.Services.AddScoped<IProjectInvitationService, ProjectInvitationService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IAuthService, OpenUpTool.Infrastructure.Services.AuthService>();
        builder.Services.AddScoped<IAuditLogService, OpenUpTool.Infrastructure.Services.AuditLogService>();
        builder.Services.AddScoped<IMicroincrementService, MicroincrementService>();
        builder.Services.AddScoped<IProjectClosureService, ProjectClosureService>();
        builder.Services.AddScoped<IFinalBuildService, FinalBuildService>();
        builder.Services.AddScoped<IWorkflowService, OpenUpTool.Infrastructure.Services.WorkflowService>();
        builder.Services.AddScoped<IWorkflowStateService, OpenUpTool.Infrastructure.Services.WorkflowStateService>();
        builder.Services.AddScoped<IArtifactStateService, OpenUpTool.Infrastructure.Services.ArtifactStateService>();
        builder.Services.AddScoped<IWorkflowPermissionService, OpenUpTool.Infrastructure.Services.WorkflowPermissionService>();
        builder.Services.AddScoped<IFileStorageService>(sp =>
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var logger = sp.GetRequiredService<ILogger<OpenUpTool.Infrastructure.Services.FileStorageService>>();
            var uploadPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
            return new OpenUpTool.Infrastructure.Services.FileStorageService(uploadPath, logger);
        });

        // Configurar JWT Authentication
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                       "your-super-secret-key-change-this-in-production-min-32-chars";
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "OpenUpTool",
                ValidAudience = "OpenUpTool",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

        builder.Services.AddAuthorization();

        // Configurar los servicios
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        
        builder.Services.AddEndpointsApiExplorer();
        
        // Configurar Swagger - siempre disponible
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() 
            { 
                Title = "OpenUpTool API", 
                Version = "v1",
                Description = "API para gestión de proyectos con metodología OpenUP",
                Contact = new()
                {
                    Name = "OpenUpTool Team",
                    Email = "admin@openuptool.com"
                }
            });
            
            // Configurar autenticación JWT en Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            // Incluir comentarios XML si existen
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
            
            // Habilitar anotaciones
            c.EnableAnnotations();
        });
        
        var app = builder.Build();

        // Comentar migraciones automáticas - usar script SQL en docker/init-scripts/
        /*
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenUpToolDbContext>();
            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Error al aplicar migraciones");
            }
        }
        */

        // Habilitar Swagger en todos los entornos
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenUpTool API v1");
            c.RoutePrefix = string.Empty; // Swagger en la raíz
            c.DocumentTitle = "OpenUpTool API Documentation";
            c.DisplayRequestDuration();
        });

        app.UseHttpsRedirection();
        
        // Middleware de logging personalizado
        app.UseMiddleware<RequestLoggingMiddleware>();
        
        app.UseCors("AllowFrontend");
        
        // Autenticación y Autorización
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        // Mostrar información de la API al iniciar
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         ✅ OpenUpTool API está corriendo!                ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("   🌐 Swagger UI: ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("http://localhost:5000");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("   📡 API:        ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("http://localhost:5000/api");
            Console.ResetColor();
            Console.WriteLine();
        });

        app.Run();
    }
}

// Hacer Program accesible para pruebas de integración
public partial class Program { }
