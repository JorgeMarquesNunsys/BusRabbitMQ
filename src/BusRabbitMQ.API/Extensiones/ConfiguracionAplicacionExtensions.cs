using BusRabbitMQ.API.Servicios;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Extensiones;
using Microsoft.OpenApi.Models;

namespace BusRabbitMQ.API.Extensiones;

public static class ConfiguracionAplicacionExtensions
{
    public static void RegistrarServiciosAplicacion(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BusRabbit API",
                Version = "v1",
                Description = "API defensiva para administrar colas RabbitMQ (send/publish/subscribe)."
            });
        });

        builder.Services.AgregarComponentesCompartidosRabbit(builder.Configuration);
        builder.Services.AddSingleton<IContextoConexionRabbit, ContextoConexionRabbit>();
        builder.Services.AddSingleton<IServicioColaRabbit, ServicioColaRabbit>();
    }

    public static void ConfigurarPipelineAplicacion(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSwagger();
        app.UseSwaggerUI(opciones =>
        {
            opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "BusRabbit API v1");
            opciones.DisplayOperationId();
        });

        app.UseHttpsRedirection();
        app.MapControllers();
    }
}