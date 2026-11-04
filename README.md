# TP_Control_de_stock_y_ventas-Ferrino-Quiroga

## Sistema de Gestión de Stock y Ventas

### Descripción

Aplicación de escritorio desarrollada en Windows Forms con conexión a base de datos mediante Entity Framework Core.

El sistema permite gestionar el stock de productos y las operaciones de ventas, al estilo de un marketplace. Organizando la información en capas con una biblioteca de clases.

### Entidades principales

- **Producto:** código, nombre, categoría, precio, stock disponible y proveedor principal.
- **Cliente:** datos personales y de contacto.
- **Venta:** fecha, cliente, detalle de productos vendidos, total.
- **Usuario:** credenciales para acceso al sistema.
- **Proveedor:** nombre o razón social, teléfono, email, dirección y estado activo.
- **Calificacion:** venta asociada, puntuación de 1 a 5, comentario opcional y fecha.

Cliente, Proveedor y Usuario serán clases independientes. Cada producto tendrá un proveedor principal; un proveedor podrá suministrar varios productos.

---

### Objetivos y Funcionalidades

El sistema busca:

- Organizar y simplificar la gestión de stock y ventas.
- Permitir operaciones CRUD (Altas, Bajas, Modificaciones, Consultas) sobre las entidades principales.
- Generar reportes básicos para análisis y control.

### Funcionalidades CRUD

**1. Productos**

- Alta de nuevos productos con stock inicial.
- Baja de productos obsoletos o discontinuados.
- Modificación de precios y stock.
- Selección de categoría desde una lista desplegable y del proveedor principal del producto.

**2. Clientes**

- Alta de clientes nuevos.
- Baja de clientes inactivos.
- Modificación de datos de contacto.

**3. Proveedores**

- Alta de nuevos proveedores.
- Baja lógica de proveedores mediante su estado activo, conservando los productos asociados.
- Modificación de datos de contacto.
- Consulta de proveedores y sus productos asociados.

### Calificación post compra

- Registro de la opinión del cliente sobre la compra completa, asociada a una venta existente.
- Puntuación entera de 1 a 5 y comentario opcional.
- Cada venta podrá tener como máximo una calificación, cargada por el usuario del sistema a partir de la opinión del cliente.

---

### Reportes previstos

El sistema incluirá al menos 5 reportes básicos:

1. Listado de productos con bajo stock (alerta de reposición).
2. Historial de ventas por cliente (detalle de compras realizadas).
3. Ventas realizadas en un período (filtrado por fechas).
4. Productos más vendidos (ranking de popularidad).
5. Inventario por categoría (cantidad de productos, unidades y valor a precio de venta).

---

### Integración de capas para guardar un registro

El sistema está organizado en capas siguiendo buenas prácticas de arquitectura:

**1. Capa de Presentación (Windows Forms)**

- El usuario completa un formulario (ejemplo: alta de producto).
- Se valida la información ingresada.

**2. Capa de Lógica de Negocio**

- Se aplican reglas de negocio (ejemplo: no permitir stock negativo).
- Se construye el objeto `Producto` con los datos recibidos.

**3. Capa de Acceso a Datos (Repositorios con Entity Framework Core)**

- El repositorio recibe el objeto y lo agrega al contexto (`DbContext`).
- Se ejecuta `SaveChanges()` para persistir el registro en la base de datos SQLite.

**4. Base de Datos (SQLite)**

- El registro queda almacenado en la tabla correspondiente.
- La información puede ser consultada

---

### Etapa 2 — Primer prototipo funcional

El prototipo está desarrollado en C# con .NET 8 y Entity Framework Core 8, usando SQLite.

- **Productos:** alta, consulta y búsqueda, modificación y baja lógica. Incluye selección de categoría y proveedor, y validación de códigos únicos, precios y stock.
- **Proveedores:** alta, consulta, modificación y baja lógica desde una ventana independiente.
- **Biblioteca de clases (`AccesoDatos`):** los siete modelos del diagrama, contexto, repositorios, servicios y migración inicial.
- **Aplicación (`StockVentas.WinForms`):** formularios de productos y proveedores. Al iniciar aplica las migraciones pendientes y conserva los datos entre ejecuciones.

En la entrega de la Etapa 2 estaban pendientes las pantallas de clientes, ventas, usuarios, calificaciones y reportes, con sus modelos y relaciones definidos. La Etapa 3 incorpora clientes, ventas y reportes.

#### Ejecución desde VS Code

Se requiere **Windows y el SDK de .NET 8** para ejecutar WinForms. Desde la terminal del repositorio:

```shell
dotnet restore StockVentas.sln
dotnet tool restore
dotnet ef database update --project src/AccesoDatos
dotnet run --project src/StockVentas.WinForms
```

También se puede compilar con `Ctrl+Shift+B` y ejecutar con `F5` usando las tareas incluidas en `.vscode`.

La base se guarda por defecto en `%LOCALAPPDATA%\StockVentas\stockventas.db`. Se puede elegir otra ruta mediante `STOCKVENTAS_DB_PATH`. Los archivos de la base no se suben al repositorio; se incluyen las migraciones para recrearla.

Para verificar el ABM y la persistencia con una base SQLite temporal:

```shell
dotnet run --project tests/StockVentas.Pruebas
```

La biblioteca, las migraciones y estas pruebas también pueden ejecutarse en macOS. La interfaz WinForms requiere Windows.

Ver la [guía de la Etapa 2](docs/Etapa2.md) para el recorrido de prueba, las migraciones y la organización del código.

### Etapa 3 — Reportes y mejoras

La segunda versión incorpora un **menú principal** con acceso a Productos, Proveedores, Clientes, Ventas y Reportes.

- **Clientes:** alta, consulta, modificación y baja lógica, con nombre y apellido obligatorios y validación de email.
- **Ventas:** selección de cliente y productos, cantidades enteras, total estimado y confirmación. Al guardar se comprueba el stock actual, se conservan los precios unitarios y se registra la venta junto con el descuento de stock en una sola transacción.
- **Reportes:** cinco consultas desde una ventana común, con filtros habilitados según corresponda, totales y aviso cuando no hay resultados.
- **Validaciones:** se mantienen las de productos y proveedores y se agregan controles de clientes, cantidades, stock, fechas y selección de filtros.
- **Prompts:** se incluyen mensajes reales utilizados durante el trabajo en [prompts.txt](prompts.txt).

Los reportes implementados son stock bajo, historial por cliente, ventas por período, productos más vendidos e inventario por categoría. Las fechas de los reportes incluyen los días completos seleccionados; los históricos conservan las compras de clientes o productos dados de baja.

Esta versión funciona como una aplicación local sin inicio de sesión: las ventas quedan asociadas a un registro interno llamado `operador-local`, creado con la primera venta. Las pantallas de autenticación y calificaciones no forman parte de esta entrega.

Se mantiene la biblioteca **AccesoDatos**, el proyecto **StockVentas.WinForms** y el esquema existente; esta etapa no necesita una migración adicional.

Ver la [guía de la Etapa 3](docs/Etapa3.md) para los filtros, las validaciones y un recorrido de demostración con resultados esperados. Los comandos de compilación, ejecución y pruebas son los mismos de la Etapa 2.

### Etapa 4 — Inyección de dependencias

La aplicación utiliza el contenedor `Microsoft.Extensions.DependencyInjection`. Los repositorios y servicios se registran en `AccesoDatos/Configuracion/ConfiguracionServicios.cs`; los formularios y la navegación, en `StockVentas.WinForms/ConfiguracionAplicacion.cs`.

Siguiendo el material de clase, los contratos se definen en `AccesoDatos/Interfaces`, con nombres como `IProductoServicio` e `IProductoRepositorio`. Las clases implementan esos contratos y los formularios y servicios los solicitan por constructor. El contenedor relaciona cada interfaz con su implementación, por ejemplo `AddScoped<IProductoServicio, ProductoServicio>()`. `Program.cs` inicia esta configuración mediante `AgregarAplicacion()`.

- Los servicios y repositorios son **Scoped**: se comparten dentro del ámbito de una ventana.
- Los formularios son **Transient**: cada apertura crea una ventana nueva dentro de su propio ámbito.
- `NavegacionFormularios` es **Singleton** y crea un ámbito por apertura, que se libera al cerrar el formulario.
- La fábrica `IDbContextFactory<StockVentasDbContext>` es compartida. Cada operación crea su propio contexto y lo libera con `using`.

`Program.cs` configura y valida el contenedor, aplica las migraciones existentes y solicita el formulario principal. La fábrica de diseño para `dotnet ef` utiliza la misma configuración de SQLite. Se mantienen las entidades, el esquema, los reportes y las validaciones.

Ver la [explicación del antes y el después y la guía de la Etapa 4](docs/Etapa4.md). Las pruebas se ejecutan con el mismo comando de las etapas anteriores e incluyen comprobaciones del contenedor y de sus tiempos de vida.

Para preparar la exposición, se incluye la [guía de estudio en Word](docs/Guia_exposicion_Etapa4.docx), con la estructura del código, ejemplos de los recorridos principales y posibles preguntas con respuestas.
