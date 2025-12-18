using BusRabbitMQ.API.Servicios;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BusRabbitMQ.APITest.Servicios;

public sealed class ContextoConexionRabbitTests
{
    private readonly Mock<IAdministradorLog> _logMock = new();

    [Fact]
    public async Task ActualizarConfiguracionAsync_DeberiaRetornarFalloCuandoConfiguracionInvalida()
    {
        var opciones = new OptionsMonitorPrueba<ConfiguracionConexionRabbit>(new ConfiguracionConexionRabbit());
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
        var configuracionInicial = new ConfiguracionConexionRabbit { NombreColaPorDefecto = "cola.default" };
        var opciones = new OptionsMonitorPrueba<ConfiguracionConexionRabbit>(configuracionInicial);
        using var contexto = new ContextoConexionRabbit(opciones, _logMock.Object);

        var configuracionTemporal = new ConfiguracionConexionRabbit { NombreColaPorDefecto = "cola.temporal" };
        await contexto.ActualizarConfiguracionAsync(configuracionTemporal, CancellationToken.None);

        var resultado = await contexto.RestablecerConfiguracionPredeterminadaAsync(CancellationToken.None);

        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor!.NombreColaPorDefecto.Should().Be("cola.default");
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