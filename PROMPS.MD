# PROMPS Y SOLUCIONES

## Entrada 1
**Prompt:** Crear la solución BusRabbit para manejar colas RabbitMQ en .NET 8 siguiendo buenas prácticas defensivas, con registro diario de errores y organización en proyectos Shared, API y APITest.

**Solución:** Se inició el proyecto Shared con modelos, enumeraciones, helpers y extensiones de inyección de dependencias necesarios para gestionar configuraciones de RabbitMQ, resultados de operaciones y registro centralizado de logs.

## Entrada 2
**Prompt:** Implementar los modelos de solicitudes en Shared, configurar la API con paquetes RabbitMQ/Swagger, servicios (`ServicioColaRabbit`), controlador (`ColasController`), configuración (`appsettings*`) y documentar README/PROMPS.

**Solución:** Se agregaron `SolicitudOperacionCola` y `SolicitudEstadoCola`, se implementaron `IServicioColaRabbit` y `ServicioColaRabbit` con conexión defensiva, QoS y requeue, se expusieron los endpoints `send`, `publish` y `subscribe`, se añadieron las configuraciones necesarias y se actualizó la documentación.

## Entrada 3
**Prompt:** Fijar `Microsoft.OpenApi` en 1.6.14, unificar el stack OpenAPI en el generador integrado, agrupar configuraciones en extensiones y eliminar dependencias redundantes.

**Solución:** Se creó `ConfiguracionAplicacionExtensions` para registrar servicios/pipeline, `Program.cs` quedó minimalista, se removió Swashbuckle para usar solo `AddOpenApi`/`MapOpenApi`, se añadió la referencia `Microsoft.OpenApi` 1.6.14, se actualizó el `.csproj`, el README y `PROMPS.MD`.

## Entrada 4
**Prompt:** Resolver la degradación del paquete `Microsoft.OpenApi` detectada por `Microsoft.AspNetCore.OpenApi` y revisar el uso de `UseSwagger` en `Program.cs`.

**Solución:** Se actualizó `Microsoft.OpenApi` a la versión 2.0.0, alineada con la dependencia mínima de `Microsoft.AspNetCore.OpenApi`, eliminando la advertencia como error. Además, `Program.cs` se simplificó para delegar todo el pipeline a `ConfiguracionAplicacionExtensions`, manteniendo únicamente el generador OpenAPI integrado (`MapOpenApi`) sin llamadas redundantes a `UseSwagger`/`UseSwaggerUI`.

## Entrada 5
**Prompt:** Restaurar Swagger en la aplicación conservando la configuración defensiva existente.

**Solución:** Se reintrodujo Swashbuckle (`AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`) mediante `ConfiguracionAplicacionExtensions`, se actualizó `Program.cs` para mantener el pipeline centralizado, se añadió la dependencia `Swashbuckle.AspNetCore`, se corrigieron las tildes en `SolicitudEstadoCola` y se documentó la disponibilidad de Swagger en `README.md`.

## Entrada 6
**Prompt:** Revisar dependencias porque faltaba algo para Swagger.

**Solución:** Se simplificó el proyecto API para depender únicamente de `Swashbuckle.AspNetCore`, eliminando referencias redundantes (`Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi`) y garantizando que `AddSwaggerGen` disponga de todas las dependencias necesarias.

## Entrada 7
**Prompt:** Mover la conexión alternativa a un endpoint dedicado de modo que, al cambiarla, los endpoints `send/publish/subscribe` usen esa cola hasta nuevo aviso.

**Solución:** Se añadió `ContextoConexionRabbit` como singleton para mantener la conexión activa y nuevos endpoints `GET/POST /api/colas/connection`. Se eliminaron las conexiones personalizadas por solicitud, se ajustaron los modelos (`SolicitudOperacionCola`, `SolicitudEstadoCola`) y `ServicioColaRabbit` para consumir la conexión del contexto, se registró el nuevo servicio de estado y se actualizó la documentación.

## Entrada 8
**Prompt:** Ajustar la API para que el cambio de conexión se haga mediante un endpoint sin body que restablezca la configuración por defecto, manteniendo la conexión alternativa en otro endpoint.

**Solución:** Se amplió `ContextoConexionRabbit` para almacenar la conexión predeterminada, exponer métodos de actualización y restablecimiento, y registrar eventos de logging. `ServicioColaRabbit` ahora consume siempre la conexión activa desde el contexto, `ColasController` ofrece `POST /api/colas/connection/default` sin body, los modelos de solicitud se limpiaron y la documentación se actualizó.

## Entrada 9
**Prompt:** Crear el proyecto de pruebas unitarias dedicado y validar controladores y servicios.

**Solución:** Se introdujo `IFabricaConexionRabbit`/`FabricaConexionRabbit` para desacoplar la creación de conexiones (facilitando los tests), se registró en el contenedor y se refactorizó `ServicioColaRabbit`. Se creó el proyecto `BusRabbitMQ.APITest` con xUnit, Moq y FluentAssertions, añadiendo pruebas para `ColasController`, `ContextoConexionRabbit` y `ServicioColaRabbit`. Se actualizó la documentación y el historial de prompts.

## Entrada 10
**Prompt:** Crear el proyecto de pruebas, agregar referencias necesarias y escribir pruebas unitarias para extensiones y helpers en archivos/carpeta separados.

**Solución:** Se añadió `BusRabbitMQ.APITest` con xUnit, FluentAssertions, Moq y coverlet; se crearon los tests `Extensiones/RegistroServiciosCompartidosExtensionsTests` (verificando DI y validaciones) y `Helpers/LogHelperTests` (asegurando la generación de logs y la omisión de mensajes vacíos).

## Entrada 11
**Prompt:** Repetir la estructura de carpetas/nombres para añadir pruebas unitarias de `ColasController`, `ServicioColaRabbit` y `ContextoConexionRabbit`.

**Solución:** Se reorganizó el árbol de pruebas (`Controladores/Colas`, `Servicios/Cola`, `Servicios/Contexto`) y se implementaron casos adicionales que cubren escenarios exitosos y de error, incluyendo mockeo detallado de conexiones RabbitMQ y verificaciones de logging defensivo.