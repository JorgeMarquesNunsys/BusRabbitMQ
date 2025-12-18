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