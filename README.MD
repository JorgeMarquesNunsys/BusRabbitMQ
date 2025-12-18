# BusRabbit

## Descripción general
BusRabbit es una solución .NET 10 orientada a exponer servicios REST que administran colas RabbitMQ mediante envío directo (`SEND`), publicación (`PUBLISH`) y suscripción (`SUBSCRIBE`). La solución aplica programación defensiva, registro diario de errores y publica un documento OpenAPI mediante el generador integrado de ASP.NET Core.

## Arquitectura
- **BusRabbitMQ.Shared**: Modelos en español, enumeraciones, helpers (por ejemplo, `LogHelper`, `EnumHelper`) y extensiones de inyección para compartir configuración de RabbitMQ y logging.
- **BusRabbitMQ.API**: Servicios que interactúan con RabbitMQ (`ServicioColaRabbit`) y controladores (`ColasController`) expuestos a través del generador OpenAPI integrado (`AddOpenApi`/`MapOpenApi`), con endpoints `send`, `publish` y `subscribe`.
- **BusRabbitMQ.APITest**: (Pendiente) Incluirá pruebas unitarias con xUnit para controladores y servicios, organizadas por modelo.

## Configuración destacada
- **Logs**: Se espera la sección `ConfiguracionLog` en `appsettings*.json` con la ruta base del log. Los archivos se generan diariamente con el formato `yyyyMMddBusRabbitMQ.log` y mensaje `TIPOERROR:: Fecha:: @CLASE:: @METODO:: Mensaje`.
- **RabbitMQ**: La sección `ConfiguracionRabbitMQ` define la conexión predeterminada, la cola por defecto, `Prefetch`, persistencia y SSL. Se admite sobreescritura por solicitud mediante `ConexionPersonalizada`.
- **OpenAPI**: El pipeline usa exclusivamente el generador integrado (`Microsoft.AspNetCore.OpenApi`) para reducir dependencias externas y tiempo de inicio; la especificación se expone vía `MapOpenApi`.

## Uso por proyecto
### BusRabbitMQ.Shared
Incluye:
- Modelos: `ConfiguracionConexionRabbit`, `ConfiguracionRegistroLog`, `MensajeGenerico<T>`, `ResultadoOperacionCola<T>`, `EstadoColaDetalle`, `SolicitudOperacionCola` y `SolicitudEstadoCola`.
- Enumeraciones `NivelLog` y `TipoOperacionCola`.
- Helpers `LogHelper` y `EnumHelper`.
- Extensión `AgregarComponentesCompartidosRabbit` para registrar configuraciones y el `LogHelper` en el contenedor de dependencias.

### BusRabbitMQ.API
- `Program.cs` delega el registro de servicios y pipeline en `ConfiguracionAplicacionExtensions`, manteniendo un único stack OpenAPI integrado.
- `IServicioColaRabbit` y `ServicioColaRabbit` encapsulan la conexión, QoS, publicación, lectura defensiva y logging ante errores.
- `ColasController` expone:
  - `POST /api/colas/send`: envía y verifica disponibilidad del mensaje, fallando si no hay suscriptores activos.
  - `POST /api/colas/publish`: publica mensajes genéricos.
  - `POST /api/colas/subscribe`: devuelve el estado de la cola, número de mensajes y contenido acotado.

### BusRabbitMQ.APITest
Pendiente de implementación: pruebas unitarias con xUnit organizadas por carpeta y archivo para cada controlador y servicio.

## Próximos pasos
1. Implementar pruebas unitarias (APITest) para servicios y controladores.
2. Completar ejemplos de uso del documento OpenAPI en distintos entornos.
3. Ajustar la configuración de RabbitMQ y logs según el entorno de despliegue.