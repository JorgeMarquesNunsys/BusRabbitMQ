using BusRabbitMQ.API.Servicios;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Extensiones;

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
        builder.Services.AddOpenApi();

        builder.Services.AgregarComponentesCompartidosRabbit(builder.Configuration);
        builder.Services.AddSingleton<IServicioColaRabbit, ServicioColaRabbit>();
    }

    public static void ConfigurarPipelineAplicacion(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseHttpsRedirection();
        app.MapControllers();
        app.MapOpenApi();
    }
}