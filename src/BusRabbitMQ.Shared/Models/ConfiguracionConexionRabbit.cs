using System.Collections.Generic;

namespace BusRabbitMQ.Shared.Models;

public sealed class ConfiguracionConexionRabbit
{
    public string NombreConexion { get; init; } = "ConexionPrincipal";
    public string HostName { get; init; } = "localhost";
    public int Puerto { get; init; } = 5672;
    public string Usuario { get; init; } = "guest";
    public string Contrasena { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string NombreColaPorDefecto { get; init; } = string.Empty;
    public bool UsarSsl { get; init; }
    public ushort Prefetch { get; init; } = 1;
    public bool PersistirMensajes { get; init; } = true;
    public int TiempoMaximoEsperaSegundos { get; init; } = 30;

    public IReadOnlyCollection<string> Validar()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(HostName))
        {
            errores.Add("El HostName de RabbitMQ es obligatorio.");
        }

        if (Puerto <= 0)
        {
            errores.Add("El puerto configurado debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(Usuario))
        {
            errores.Add("El usuario de RabbitMQ es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(Contrasena))
        {
            errores.Add("La contraseña de RabbitMQ es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(NombreColaPorDefecto))
        {
            errores.Add("Debe definirse la cola predeterminada.");
        }

        if (Prefetch == 0)
        {
            errores.Add("El valor de Prefetch debe ser mayor o igual a uno.");
        }

        if (TiempoMaximoEsperaSegundos <= 0)
        {
            errores.Add("El tiempo máximo de espera debe ser mayor a cero.");
        }

        return errores;
    }
}