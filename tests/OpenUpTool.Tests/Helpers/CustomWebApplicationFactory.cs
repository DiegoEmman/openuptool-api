using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenUpTool.Infrastructure.Data;
using OpenUpTool.Api;

namespace OpenUpTool.Tests.Helpers;

/// <summary>
/// Factory personalizada para crear una aplicación de pruebas con base de datos en memoria
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remover el DbContext existente
            services.RemoveAll(typeof(DbContextOptions<OpenUpToolDbContext>));

            // Agregar DbContext con base de datos en memoria
            services.AddDbContext<OpenUpToolDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });

            // Construir el service provider
            var sp = services.BuildServiceProvider();

            // Crear un scope para obtener el DbContext y sembrar datos
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<OpenUpToolDbContext>();

            // Asegurarse de que la base de datos esté creada
            db.Database.EnsureCreated();
        });
    }
}
