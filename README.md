# MultibodegaAPIS

**Autor:** Jorge Acosta  
**Tecnologias:** C#, ASP.NET Core Web API (.NET 10), Entity Framework Core, SQL Server, OpenAPI/Swagger UI.

## Descripcion
Proyecto de servicios web para gestionar un sistema multibodega. La solucion de Visual Studio `MultibodegaAPIS.slnx` incluye **tres APIs** que intercambian informacion mediante solicitudes HTTP.

| Servicio | Proyecto | Operaciones observadas en el codigo |
| --- | --- | --- |
| Catalogo | `Catalogo.Api/` | Categorias, marcas, productos, consulta de existencias |
| Inventario | `Inventario.Api/` | Bodegas, existencias, movimientos y reservas |
| Pedidos | `Pedidos.Api/` | Clientes, pedidos y estados de pedidos |

Existe ademas la carpeta `MultibodegaAPIS/`, con un proyecto Web API de ejemplo. No forma parte de la solucion `.slnx` suministrada.

## Requisitos

- Visual Studio compatible con el formato de solucion `.slnx`.
- SDK de **.NET 10**.
- SQL Server con las bases de datos y tablas necesarias para el proyecto.

## Abrir y ejecutar

1. Abrir `MultibodegaAPIS.slnx` con Visual Studio.
2. Verificar o adaptar las cadenas de conexion `ConnectionStrings` en los archivos `appsettings.json` de cada API.
3. Asegurarse de que existan las bases de datos y tablas necesarias: `CatalogoDB`, `InventarioDB` y `PedidosDB` segun las configuraciones del proyecto.
4. Configurar el inicio de multiples proyectos para ejecutar Catalogo, Inventario y Pedidos al mismo tiempo.
5. Iniciar las APIs. En entorno Development, cada una registra el documento OpenAPI y la interfaz Swagger UI en la ruta principal `/`.

Los perfiles HTTP del ZIP original utilizan estas direcciones locales (verificar que esten libres):

- Catalogo: `http://localhost:5294`
- Inventario: `http://localhost:5148`
- Pedidos: `http://localhost:5031`

Las direcciones de otros servicios se configuran en la seccion `Servicios` de cada `appsettings.json`.

> **Importante:** El archivo ZIP entregado no contiene scripts SQL para crear la estructura de las bases de datos. Para ejecutar completamente el proyecto hace falta disponer de dichas estructuras en SQL Server. Las cadenas de conexion incluidas utilizan autenticacion integrada de Windows para un servidor local; actualizarlas si tu entorno es diferente.

## Estructura

```text
MultibodegaAPIS/
├── Catalogo.Api/
├── Inventario.Api/
├── Pedidos.Api/
├── MultibodegaAPIS/          # Proyecto de ejemplo no incluido en .slnx
├── MultibodegaAPIS.slnx     # Solucion de tres APIs
├── .gitignore
└── README.md
```

## Repositorio y seguridad

La carpeta `.vs/` y los directorios `bin/` y `obj/` no se suben porque son archivos temporales o resultados de compilacion. Antes de publicar, revisa los archivos de configuracion para comprobar que no contienen informacion privada. El PDF academico adjunto al ZIP original se ha excluido del repositorio publico. Nunca publiques claves, contrasenas o tokens.
