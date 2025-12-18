using BusRabbitMQ.API.Servicios;
using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BusRabbitMQ.APITest.Servicios.Contexto;

public sealed class ContextoConexionRabbitTests
{
    private readonly Mock<IAdministradorLog> _logMock = new();

    [Fact]
    public async Task ActualizarConfiguracionAsync_DeberiaActualizarConexionYRegistrarEvento()
    {
        var opciones = new OptionsMonitorPrueba<ConfiguracionConexionRabbit>(CrearConfiguracion("cola.default"));
        using var contexto = new ContextoConexionRabbit(opciones, _logMock.Object);
        var nuevaConfiguracion = CrearConfiguracion("cola.alternativa");

        var resultado = await contexto.ActualizarConfiguracionAsync(nuevaConfiguracion, CancellationToken.None);

        resultado.EsExitoso.Should().BeTrue();
        contexto.ObtenerConfiguracionActual().NombreColaPorDefecto.Should().Be("cola.alternativa");
        _logMock.Verify(l => l.RegistrarEventoAsync(
                NivelLog.Informacion,
                nameof(ContextoConexionRabbit),
                nameof(ContextoConexionRabbit.ActualizarConfiguracionAsync),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarConfiguracionAsync_CuandoConfigInvalida_DeberiaRetornarFallo()
    {
        var opciones = new OptionsMonitorPrueba<ConfiguracionConexionRabbit>(CrearConfiguracion("cola.default"));
        using var contexto = new ContextoConexionRabbit(opciones, _logMock.Object);
        var configuracionInvalida = new ConfiguracionConexionRabbit
        {
            NombreColaPorDefecto = string.Empty,
            Prefetch = 0
        };

        var resultado = await contexto.ActualizarConfiguracionAsync(configuracionInvalida, CancellationToken.None);

        resultado.EsExitoso.Should().BeFalse();
        resultado.Errores.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RestablecerConfiguracionPredeterminadaAsync_DeberiaRestaurarValoresIniciales()
    {
        var configuracionInicial = CrearConfiguracion("cola.default");
        var opciones = new OptionsMonitorPrueba<ConfiguracionConexionRabbit>(configuracionInicial);
        using var contexto = new ContextoConexionRabbit(opciones, _logMock.Object);

        var configuracionTemporal = CrearConfiguracion("cola.temporal");
        await contexto.ActualizarConfiguracionAsync(configuracionTemporal, CancellationToken.None);

        var resultado = await contexto.RestablecerConfiguracionPredeterminadaAsync(CancellationToken.None);

        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor!.NombreColaPorDefecto.Should().Be("cola.default");
    }

    private static ConfiguracionConexionRabbit CrearConfiguracion(string nombreCola)
    {
        return new ConfiguracionConexionRabbit
        {
            NombreColaPorDefecto = nombreCola,
            HostName = "localhost",
            Usuario = "guest",
            Contrasena = "guest",
            Prefetch = 1
        };
    }

    private sealed class OptionsMonitorPrueba<T> : IOptionsMonitor<T>
    {
        private T _valorActual;

        public OptionsMonitorPrueba(T valorInicial)
        {
            _valorActual = valorInicial;
        }

        public T CurrentValue => _valorActual;

        public T Get(string? name) => _valorActual;

        public IDisposable OnChange(Action<T, string?> listener) => new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

