using BusRabbitMQ.Shared.Helpers;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusRabbitMQ.Shared.Extensiones;

public static class RegistroServiciosCompartidosExtensions
{
    public static IServiceCollection AgregarComponentesCompartidosRabbit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ConfiguracionConexionRabbit>(configuration.GetSection("ConfiguracionRabbitMQ"));
        services.Configure<ConfiguracionRegistroLog>(configuration.GetSection("ConfiguracionLog"));
        services.AddSingleton<IAdministradorLog, LogHelper>();

        return services;
    }
}