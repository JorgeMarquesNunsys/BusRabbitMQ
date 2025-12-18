# BusRabbit

## Descripción general
BusRabbit es una solución .NET 10 orientada a exponer servicios REST que administran colas RabbitMQ mediante envío directo (`SEND`), publicación (`PUBLISH`) y suscripción (`SUBSCRIBE`). La solución aplica programación defensiva, registro diario de errores, permite cambiar/restaurar la conexión activa a la cola objetivo mediante endpoints dedicados y publica la especificación Swagger (Swashbuckle) junto con la interfaz `Swagger UI` en `/swagger`.

## Arquitectura
- **BusRabbitMQ.Shared**: Modelos en español, enumeraciones, helpers (por ejemplo, `LogHelper`, `EnumHelper`) y extensiones de inyección para compartir configuración de RabbitMQ y logging. Este proyecto permanece libre de dependencias directas hacia `RabbitMQ.Client`; toda la creación de conexiones se delega a la API.
- **BusRabbitMQ.API**: Servicios que interactúan con RabbitMQ (`ContextoConexionRabbit`, `FabricaConexionRabbit`, `ServicioColaRabbit`) y controladores (`ColasController`) expuestos en Swagger. `FabricaConexionRabbit` encapsula la creación de `IConnection` y mantiene aislada la dependencia con `RabbitMQ.Client` dentro de la API.
- **BusRabbitMQ.APITest**: Pruebas unitarias con xUnit organizadas por carpetas (`Controladores`, `Servicios`). Incluyen verificaciones para `ColasController`, `ContextoConexionRabbit` y `ServicioColaRabbit`, empleando Moq y FluentAssertions.

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
- `ContextoConexionRabbit` mantiene la conexión/cola activa.
- `FabricaConexionRabbit` encapsula la creación de conexiones `RabbitMQ.Client`.
- `IServicioColaRabbit`/`ServicioColaRabbit` operan siempre contra la conexión activa.
- `ColasController` expone:
  - `GET /api/colas/connection`: obtiene la conexión actual.
  - `POST /api/colas/connection`: actualiza la conexión activa a una cola alternativa.
  - `POST /api/colas/connection/default`: restablece la conexión a la configuración definida en `appsettings`.
  - `POST /api/colas/send`: envía y verifica disponibilidad del mensaje, fallando si no hay suscriptores activos.
  - `POST /api/colas/publish`: publica mensajes genéricos.
  - `POST /api/colas/subscribe`: devuelve el estado de la cola, número de mensajes y contenido acotado.

### BusRabbitMQ.APITest
- **Controladores**: pruebas para `ColasController` asegurando respuestas `Ok/BadRequest` según los resultados de los servicios simulados.
- **Servicios**: pruebas para `ContextoConexionRabbit` (validaciones y restablecimiento) y para `ServicioColaRabbit` (validaciones y manejo de errores al publicar), usando Moq y FluentAssertions.

## Próximos pasos
1. Ejecutar las pruebas con `dotnet test src/BusRabbitMQ.APITest/BusRabbitMQ.APITest.csproj`.
2. Añadir casos adicionales para escenarios exitosos de `ServicioColaRabbit` cuando se disponga de un broker de pruebas.
3. Ajustar la configuración de RabbitMQ y logs según el entorno de despliegue.