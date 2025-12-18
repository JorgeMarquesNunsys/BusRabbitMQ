using System.Text.Json;
using BusRabbitMQ.API.Controllers;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BusRabbitMQ.APITest.Controladores.Colas;

public sealed class ColasControllerTests
{
    private readonly Mock<IServicioColaRabbit> _servicioMock = new();
    private readonly Mock<IContextoConexionRabbit> _contextoMock = new();
    private readonly ColasController _controller;

    public ColasControllerTests()
    {
        _controller = new ColasController(_servicioMock.Object, _contextoMock.Object);
    }

    [Fact]
    public void ObtenerConexionActual_DeberiaRetornarOkConConfiguracion()
    {
        var configuracion = new ConfiguracionConexionRabbit { NombreColaPorDefecto = "cola.principal" };
        _contextoMock.Setup(c => c.ObtenerConfiguracionActual()).Returns(configuracion);

        var respuesta = _controller.ObtenerConexionActual();

        var ok = Assert.IsType<OkObjectResult>(respuesta.Result);
        ok.Value.Should().BeSameAs(configuracion);
    }

    [Fact]
    public async Task ActualizarConexionAsync_CuandoOperacionFalla_DeberiaRetornarBadRequest()
    {
        var configuracion = new ConfiguracionConexionRabbit { NombreColaPorDefecto = "cola.alternativa" };
        var fallo = ResultadoOperacionCola<ConfiguracionConexionRabbit>.CrearFallo(new[] { "error" }, "fallo");
        _contextoMock
            .Setup(c => c.ActualizarConfiguracionAsync(configuracion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fallo);

        var respuesta = await _controller.ActualizarConexionAsync(configuracion, CancellationToken.None);

        respuesta.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RestablecerConexionPredeterminadaAsync_DeberiaRetornarOk()
    {
        var exito = ResultadoOperacionCola<ConfiguracionConexionRabbit>.CrearExito(new ConfiguracionConexionRabbit());
        _contextoMock
            .Setup(c => c.RestablecerConfiguracionPredeterminadaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exito);

        var respuesta = await _controller.RestablecerConexionPredeterminadaAsync(CancellationToken.None);

        respuesta.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EnviarAsync_CuandoServicioFalla_DeberiaRetornarBadRequest()
    {
        var solicitud = CrearSolicitudOperacionValida();
        var fallo = ResultadoOperacionCola<string>.CrearFallo(new[] { "error" });
        _servicioMock
            .Setup(s => s.EnviarAsync(solicitud, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fallo);

        var respuesta = await _controller.EnviarAsync(solicitud, CancellationToken.None);

        respuesta.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PublicarAsync_CuandoServicioEsExitoso_DeberiaRetornarOk()
    {
        var solicitud = CrearSolicitudOperacionValida();
        var exito = ResultadoOperacionCola<bool>.CrearExito(true);
        _servicioMock
            .Setup(s => s.PublicarAsync(solicitud, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exito);

        var respuesta = await _controller.PublicarAsync(solicitud, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(respuesta);
        ok.Value.Should().Be(exito);
    }

    [Fact]
    public async Task SuscribirAsync_CuandoServicioEsExitoso_DeberiaRetornarOk()
    {
        var solicitud = CrearSolicitudEstadoValida();
        var exito = ResultadoOperacionCola<EstadoColaDetalle>.CrearExito(new EstadoColaDetalle());
        _servicioMock
            .Setup(s => s.SuscribirAsync(solicitud, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exito);

        var respuesta = await _controller.SuscribirAsync(solicitud, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(respuesta);
        ok.Value.Should().Be(exito);
    }

    [Fact]
    public async Task SuscribirAsync_CuandoServicioFalla_DeberiaRetornarBadRequest()
    {
        var solicitud = CrearSolicitudEstadoValida();
        var fallo = ResultadoOperacionCola<EstadoColaDetalle>.CrearFallo(new[] { "error" });
        _servicioMock
            .Setup(s => s.SuscribirAsync(solicitud, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fallo);

        var respuesta = await _controller.SuscribirAsync(solicitud, CancellationToken.None);

        respuesta.Should().BeOfType<BadRequestObjectResult>();
    }

    private static SolicitudOperacionCola CrearSolicitudOperacionValida()
    {
        using var doc = JsonDocument.Parse("{\"mensaje\":\"hola\"}");
        return new SolicitudOperacionCola
        {
            NombreCola = "cola.test",
            Contenido = doc.RootElement.Clone(),
            PermitirPublicarSinSuscriptores = true
        };
    }

    private static SolicitudEstadoCola CrearSolicitudEstadoValida()
    {
        return new SolicitudEstadoCola
        {
            NombreCola = "cola.test",
            IncluirContenido = true,
            MaximoMensajes = 1
        };
    }
}