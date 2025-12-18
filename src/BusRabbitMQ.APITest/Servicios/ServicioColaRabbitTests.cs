using System.Text.Json;
using BusRabbitMQ.API.Servicios;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusRabbitMQ.APITest.Servicios;

public sealed class ServicioColaRabbitTests
{
    private readonly Mock<IContextoConexionRabbit> _contextoMock = new();
    private readonly Mock<IAdministradorLog> _logMock = new();
    private readonly Mock<IFabricaConexionRabbit> _fabricaMock = new();
    private readonly Mock<ILogger<ServicioColaRabbit>> _loggerMock = new();
    private readonly ServicioColaRabbit _servicio;

    public ServicioColaRabbitTests()
    {
        _servicio = new ServicioColaRabbit(
            _contextoMock.Object,
            _logMock.Object,
            _fabricaMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EnviarAsync_CuandoSolicitudInvalida_DeberiaRetornarFallo()
    {
        var resultado = await _servicio.EnviarAsync(new SolicitudOperacionCola(), CancellationToken.None);

        resultado.EsExitoso.Should().BeFalse();
        resultado.Errores.Should().Contain(e => e.Contains("contenido", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PublicarAsync_CuandoConexionFalla_DeberiaRegistrarError()
    {
        var solicitud = CrearSolicitudValida();
        var configuracion = new ConfiguracionConexionRabbit { NombreColaPorDefecto = "cola.principal" };

        _contextoMock.Setup(c => c.ObtenerConfiguracionActual()).Returns(configuracion);
        _fabricaMock
            .Setup(f => f.CrearConexion(configuracion))
            .Throws(new InvalidOperationException("Fallo de conexión"));

        var resultado = await _servicio.PublicarAsync(solicitud, CancellationToken.None);

        resultado.EsExitoso.Should().BeFalse();
        _logMock.Verify(l => l.RegistrarEventoAsync(
                NivelLog.Error,
                nameof(ServicioColaRabbit),
                nameof(ServicioColaRabbit.PublicarAsync),
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SolicitudOperacionCola CrearSolicitudValida()
    {
        using var doc = JsonDocument.Parse("{\"mensaje\":\"hola\"}");
        return new SolicitudOperacionCola
        {
            Contenido = doc.RootElement.Clone(),
            NombreCola = "cola.principal",
            PermitirPublicarSinSuscriptores = true
        };
    }
}