using System.Text;
using System.Text.Json;
using BusRabbitMQ.API.Servicios;
using BusRabbitMQ.API.Servicios.Interfaces;
using BusRabbitMQ.Shared.Enumeraciones;
using BusRabbitMQ.Shared.Interfaces;
using BusRabbitMQ.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace BusRabbitMQ.APITest.Servicios.Cola;

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
    public async Task EnviarAsync_CuandoNoExistenConsumidores_DeberiaRetornarFallo()
    {
        var configuracion = CrearConfiguracion();
        _contextoMock.Setup(c => c.ObtenerConfiguracionActual()).Returns(configuracion);

        var (_, canalMock) = PrepararConexion(configuracion);
        canalMock.Setup(m => m.QueueDeclarePassive("cola.principal"))
            .Returns(new QueueDeclareOk("cola.principal", 1, 0));
        canalMock.Setup(m => m.CreateBasicProperties()).Returns(CrearPropiedadesBasicas());

        var solicitud = CrearSolicitudOperacionValida();

        var resultado = await _servicio.EnviarAsync(solicitud, CancellationToken.None);

        resultado.EsExitoso.Should().BeFalse();
        resultado.Errores.Should().Contain(e => e.Contains("El mensaje no se encuentra disponible en la cola.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EnviarAsync_CuandoMensajeDisponible_DeberiaRetornarExito()
    {
        var configuracion = CrearConfiguracion();
        _contextoMock.Setup(c => c.ObtenerConfiguracionActual()).Returns(configuracion);

        var (_, canalMock) = PrepararConexion(configuracion);
        canalMock.Setup(m => m.QueueDeclarePassive("cola.principal"))
            .Returns(new QueueDeclareOk("cola.principal", 2, 1));
        canalMock.Setup(m => m.CreateBasicProperties()).Returns(CrearPropiedadesBasicas());

        var cuerpo = Encoding.UTF8.GetBytes("\"mensaje\"");
        var resultadoGet = new BasicGetResult(
            deliveryTag: 1,
            redelivered: false,
            exchange: string.Empty,
            routingKey: string.Empty,
            messageCount: 1,
            basicProperties: CrearPropiedadesBasicas(),
            body: new ReadOnlyMemory<byte>(cuerpo));

        canalMock.Setup(m => m.BasicGet("cola.principal", false)).Returns(resultadoGet);
        canalMock.Setup(m => m.BasicNack(1, false, true));

        var resultado = await _servicio.EnviarAsync(CrearSolicitudOperacionValida(), CancellationToken.None);

        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().Be("\"mensaje\"");
        canalMock.Verify(m => m.BasicNack(1, false, true), Times.Once);
    }

    [Fact]
    public async Task PublicarAsync_CuandoConexionFalla_DeberiaRegistrarError()
    {
        var configuracion = CrearConfiguracion();
        _contextoMock.Setup(c => c.ObtenerConfiguracionActual()).Returns(configuracion);
        _fabricaMock
            .Setup(f => f.CrearConexion(configuracion))
            .Throws(new InvalidOperationException("Fallo de conexión"));

        var resultado = await _servicio.PublicarAsync(CrearSolicitudOperacionValida(), CancellationToken.None);

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

    private (Mock<IConnection> Conexion, Mock<IModel> Canal) PrepararConexion(ConfiguracionConexionRabbit configuracion)
    {
        var conexionMock = new Mock<IConnection>();
        var canalMock = new Mock<IModel>();
        conexionMock.Setup(c => c.CreateModel()).Returns(canalMock.Object);
        _fabricaMock.Setup(f => f.CrearConexion(configuracion)).Returns(conexionMock.Object);
        return (conexionMock, canalMock);
    }

    private static ConfiguracionConexionRabbit CrearConfiguracion()
    {
        return new ConfiguracionConexionRabbit
        {
            NombreColaPorDefecto = "cola.principal",
            HostName = "localhost",
            Usuario = "guest",
            Contrasena = "guest",
            Prefetch = 1
        };
    }

    private static SolicitudOperacionCola CrearSolicitudOperacionValida()
    {
        using var doc = JsonDocument.Parse("{\"mensaje\":\"hola\"}");
        return new SolicitudOperacionCola
        {
            NombreCola = "cola.principal",
            Contenido = doc.RootElement.Clone(),
            PermitirPublicarSinSuscriptores = true
        };
    }

    private static IBasicProperties CrearPropiedadesBasicas()
    {
        return Mock.Of<IBasicProperties>();
    }
}