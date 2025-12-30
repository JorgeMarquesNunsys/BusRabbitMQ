@workspace ===CONTEXTO===
•	Eres un programador de .Net que aplica buenas practicas de programación con los mejores avances de .Net 8.
•	Ejecutas programación defensiva para capturar errores y loguearlos en archivos diarios
•	Utilizaremos siempre programación defensiva orientada a detectar posibles errores, valores nulos, valores vacios y excepciones
•	Loguearemos todos los errores en un archivo log,(Configurado en el appsettings) con generacion diaria y formato de nombre con (fecha(yyyyMMdd)+ BusRabbitMQ.log) y los mensajes como "TIPOERROR:: Fecha:: @CLASE:: @METODO:: Mensaje"
•	La aplicacion no tiene front, pero tendra swagger
•	Quiero que las clases tengan nobres representativos en español
===FINCONTEXTO===
Quiero crear una nueva solucion "BusRabbit" para manejar las colas de RabbitMQ en .Net 8,
para envio de mensajes a la cola, recuperacion de mensajes de la cola y observacion mediante suscripcion del contenido de la cola.
necesito que cumpla una serie de caracteristicas: 

===INTRUCCIONES===
•	Dividiremos la solucion en 3 proyectos -> Shared, API, APITest (Test Unitarios para la API con XUnit) Que ya estan creados en @workspace
•	Iremos creando paso a paso haciendo una pausa tras cada proyecto
•	Instalaremos los paquetes necesarios de rabbitMQ y utilizaremos los objetos genericos que podamos 
•	En Shared iran los Modelos, los metodos especificos para la inyeccion de depencias, enumerados y helpers como el LogHelper y el EnumHelper
•	En la API crearemos los servicios que atacaran a la cola de Rabbit y los controlladores publicos que usaremos desde Swagger
• 	En APITest generaremos Test Unitarios tanto para controlador como para cada servicio separados en archivos y en carpetas por cada Modelo
•	Tendremos una cola default configurada en el appsetings. Pero añadiremos un endpoint para poder consultar otras colas(CurrentConexion) con un objeto de conexion RabbitMQ
•	La conexion a una cola distinta a la default a de ser un endpoint a parte. En el caso de haber conectado a otra cola, la aplicacion ejecutara los endpoints de SEND, publish y subscribe siempre contra esa cola
•	Necesito un endpoint SEND para enviar objetos genericos a la cola que se quedara a la escucha y devolvera el objeto que se encuentra en la cola 
•	Otro Endpoint PUBLISH que simplemente enviara objetos a la cola.
•	Otro Endpoint SUSCRIBE que debera mostrar en tiempo real el contenido de la cola. Numero de paquetes y contenido de los mismos.
•	Quiero que los mensajes en la cola se queden, por ejemplo, si haces un send y no hay ningún suscriptor, que eso debería dar un error. 
===FIN INTRUCCIONES===

==MODELOS== 
Me crearas cada modelo en un archivo individual bajo un mismo namespace para todos dentro de una carpeta Models en el proyecto shared.

===FIN MODELOS===
===ARCHIVO TEXTO=== 
Quiero que me generes en la solucion un archivo de texto de tipo MD con todos los PROMPS y soluciones de este chat llamado PROMPS.MD 
Genera otro archivo README.MD con la descripcion del proyectom arquitectura y uso en cada proyecto
===FIN ARCHIVO TEXTO===


--CREATE SOLUTION
dotnet new sln -n BusRabbitMQ

--CREATE SHARED
dotnet new classlib -n BusRabbitMQ.Shared -o src/BusRabbitMQ.Shared

--CREATE API
dotnet new webapi -n BusRabbitMQ.API -o src/BusRabbitMQ.API

--References
dotnet sln add src/BusRabbitMQ.Shared src/BusRabbitMQ.API 
dotnet add src/BusRabbitMQ.API reference src/BusRabbitMQ.Shared
