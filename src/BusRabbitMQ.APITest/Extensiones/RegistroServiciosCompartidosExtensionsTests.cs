using BusRabbitMQ.Shared.Extensiones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace BusRabbitMQ.APITest.Extensiones;

public sealed class RegistroServiciosCompartidosExtensionsTests
{
    [Fact]
    public void AgregarComponentesCompartidosRabbit_DeberiaRegistrarOpcionesYLogHelper()
    {
        var valores = new Dictionary<string, string?>
        {
            ["ConfiguracionRabbitMQ:NombreColaPorDefecto"] = "cola.principal",
            ["ConfiguracionLog:DirectorioBase"] = "logs-tests"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(valores!)
            .Build();

        var services = new ServiceCollection();

        services.AgregarComponentesCompartidosRabbit(configuration);

        using var provider = services.BuildServiceProvider();
        var log = provider.GetService<IAdministradorLog>();
        log.Should().NotBeNull();

        var opcionesRabbit = provider.GetService<IOptionsMonitor<ConfiguracionConexionRabbit>>();
        opcionesRabbit.Should().NotBeNull();
        opcionesRabbit!.CurrentValue.NombreColaPorDefecto.Should().Be("cola.principal");
    }

    [Fact]
    public void AgregarComponentesCompartidosRabbit_CuandoServicesEsNulo_DeberiaLanzar()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var accion = () => RegistroServiciosCompartidosExtensions
            .AgregarComponentesCompartidosRabbit(null!, configuration);

        accion.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("services");
    }

    [Fact]
    public void AgregarComponentesCompartidosRabbit_CuandoConfigurationEsNulo_DeberiaLanzar()
    {
        var services = new ServiceCollection();

        var accion = () => services.AgregarComponentesCompartidosRabbit(null!);

        accion.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("configuration");
    }
}