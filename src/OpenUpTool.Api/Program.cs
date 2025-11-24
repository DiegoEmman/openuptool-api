using DotNetEnv;

namespace OpenUpTool.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Cargar variables de entorno desde .env
        Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        // Configurar los servicios
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Configuración de Swagger se hará en la extensión correspondiente
        
        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
