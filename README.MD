# BusRabbit

## Descripción general
BusRabbit es una solución .NET 10 orientada a exponer servicios REST que administran colas RabbitMQ mediante envío directo (`SEND`), publicación (`PUBLISH`) y suscripción (`SUBSCRIBE`). La solución aplica programación defensiva, registro diario de errores, permite cambiar la conexión activa a una cola remota mediante un endpoint dedicado o restablecerla a la configuración por defecto (sin body) y publica la especificación Swagger (Swashbuckle) junto con la interfaz `Swagger UI` en `/swagger`.

## Arquitectura
- **BusRabbitMQ.Shared**: Modelos en español, enumeraciones, helpers (por ejemplo, `LogHelper`, `EnumHelper`) y extensiones de inyección para compartir configuración de RabbitMQ y logging.
- **BusRabbitMQ.API**: Servicios que interactúan con RabbitMQ (`ContextoConexionRabbit`, `ServicioColaRabbit`) y controladores (`ColasController`) expuestos en Swagger.
- **BusRabbitMQ.APITest**: (Pendiente) Incluirá pruebas unitarias con xUnit para controladores y servicios, organizadas por modelo.

## Configuración destacada
- **Logs**: Se espera la sección `ConfiguracionLog` en `appsettings*.json` con la ruta base del log. Los archivos se generan diariamente con el formato `yyyyMMddBusRabbitMQ.log` y mensaje `TIPOERROR:: Fecha:: @CLASE:: @METODO:: Mensaje`.
- **RabbitMQ**: La sección `ConfiguracionRabbitMQ` define la conexión predeterminada, la cola por defecto, `Prefetch`, persistencia y SSL. La aplicación puede:
  - `POST /api/colas/connection`: reemplazar la conexión activa.
  - `POST /api/colas/connection/default`: restablecer la conexión activa a la configuración del `appsettings` sin enviar body.
- **Swagger**: Generado con Swashbuckle (`AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`).

## Uso por proyecto
### BusRabbitMQ.Shared
Incluye:
- Modelos: `ConfiguracionConexionRabbit`, `ConfiguracionRegistroLog`, `MensajeGenerico<T>`, `ResultadoOperacionCola<T>`, `EstadoColaDetalle`, `SolicitudOperacionCola` y `SolicitudEstadoCola`.
- Enumeraciones `NivelLog` y `TipoOperacionCola`.
- Helpers `LogHelper` y `EnumHelper`.
- Extensión `AgregarComponentesCompartidosRabbit`.

### BusRabbitMQ.API
- `Program.cs` delega el registro de servicios y pipeline en `ConfiguracionAplicacionExtensions`.
- `ContextoConexionRabbit` mantiene la conexión/cola activa (default u otra configurada mediante endpoints).
- `IServicioColaRabbit`/`ServicioColaRabbit` operan siempre contra la conexión activa.
- `ColasController` expone:
  - `GET /api/colas/connection`: obtiene la conexión actual.
  - `POST /api/colas/connection`: actualiza la conexión activa a una cola alternativa.
  - `POST /api/colas/connection/default`: restablece la conexión a la configuración definida en `appsettings`.
  - `POST /api/colas/send`: envía y verifica disponibilidad del mensaje, fallando si no hay suscriptores activos.
  - `POST /api/colas/publish`: publica mensajes genéricos.
  - `POST /api/colas/subscribe`: devuelve el estado de la cola, número de mensajes y contenido acotado.

### BusRabbitMQ.APITest
Pendiente de implementación.

## Próximos pasos
1. Implementar pruebas unitarias (APITest) para servicios, controlador y el contexto de conexión.
2. Añadir ejemplos Swagger adicionales si se requieren contratos específicos.
3. Ajustar la configuración de RabbitMQ y logs según el entorno de despliegue.