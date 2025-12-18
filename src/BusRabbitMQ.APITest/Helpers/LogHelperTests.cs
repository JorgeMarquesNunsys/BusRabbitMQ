using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Helpers;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BusRabbitMQ.APITest.Helpers;

public sealed class LogHelperTests : IDisposable
{
    private readonly string _directorioTemporal;
    private readonly LogHelper _logHelper;

    public LogHelperTests()
    {
        _directorioTemporal = Path.Combine(Path.GetTempPath(), $"logs-busrabbit-{Guid.NewGuid():N}");
        var opciones = new OptionsMonitorPrueba<ConfiguracionRegistroLog>(
            new ConfiguracionRegistroLog { DirectorioBase = _directorioTemporal, IncluirStackTrace = false });

        _logHelper = new LogHelper(opciones);
    }

    [Fact]
    public async Task RegistrarEventoAsync_DeberiaCrearArchivoConMensaje()
    {
        await _logHelper.RegistrarEventoAsync(
            NivelLog.Error,
            "ClasePrueba",
            "MetodoPrueba",
            "Mensaje esperado");

        Directory.Exists(_directorioTemporal).Should().BeTrue();

        var archivo = Directory.GetFiles(_directorioTemporal, "*BusRabbitMQ.log").Should().ContainSingle().Subject;
        var contenido = await File.ReadAllTextAsync(archivo);

        contenido.Should().Contain("Mensaje esperado");
        contenido.Should().Contain("@ClasePrueba");
        contenido.Should().Contain("@MetodoPrueba");
    }

    [Fact]
    public async Task RegistrarEventoAsync_CuandoMensajeVacio_NoDebeGenerarArchivo()
    {
        await _logHelper.RegistrarEventoAsync(NivelLog.Informacion, "Clase", "Metodo", string.Empty);

        Directory.Exists(_directorioTemporal).Should().BeFalse();
    }

    public void Dispose()
    {
        _logHelper.Dispose();
        if (Directory.Exists(_directorioTemporal))
        {
            Directory.Delete(_directorioTemporal, true);
        }
    }

    private sealed class OptionsMonitorPrueba<T> : IOptionsMonitor<T>
    {
        private T _valor;

        public OptionsMonitorPrueba(T valorInicial)
        {
            _valor = valorInicial;
        }

        public T CurrentValue => _valor;

        public T Get(string? name) => _valor;

        public IDisposable OnChange(Action<T, string?> listener) => new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}