# Etapa 4 — Inyección de dependencias

Consigna: generar la inyección de dependencias. Fecha indicada: **04/11/2026**.

## Antes y después

| Aspecto | Antes de esta etapa | Después de esta etapa |
|---|---|---|
| Creación de objetos | `Program.cs` construía repositorios y servicios con `new` y los pasaba al formulario principal. | El contenedor construye las clases según registros centralizados. |
| Tipo de inyección | Inyección manual por constructor. | Inyección por constructor resuelta por el contenedor de .NET. |
| Apertura de ventanas | El menú y Productos creaban otras ventanas con `new`. | `NavegacionFormularios` crea un ámbito y obtiene cada formulario del contenedor. |
| Contextos | Los repositorios recibían un `Func<StockVentasDbContext>`. | Reciben `IDbContextFactory<StockVentasDbContext>`. |
| Tiempo de vida | Las instancias se conectaban manualmente y las ventanas se liberaban con `using`. | Los ámbitos administran las dependencias de cada ventana. Los contextos creados por la fábrica se siguen liberando con `using`. |
| Registro de dependencias | Estaba distribuido entre el arranque y los formularios. | Está centralizado en las configuraciones de AccesoDatos y WinForms. |

Los constructores ya recibían dependencias en la versión anterior. El cambio de esta etapa consiste en delegar su construcción y administración al contenedor.

Tras revisar el material de la materia, se incorporaron interfaces propias para seguir también el enfoque de contrato e implementación que se muestra en clase.

## Archivos que explican el cambio

1. **`src/AccesoDatos/Configuracion/ConfiguracionServicios.cs`:** registra la fábrica de contextos y las relaciones entre las interfaces y las implementaciones de los cinco repositorios y cinco servicios. El método `AgregarAccesoDatos` también admite una configuración de base alternativa para las pruebas.
2. **`src/StockVentas.WinForms/ConfiguracionAplicacion.cs`:** reúne la configuración de datos, el navegador y los seis formularios.
3. **`src/StockVentas.WinForms/Program.cs`:** construye el contenedor con `ValidateOnBuild` y `ValidateScopes`, inicializa la base mediante la fábrica registrada y solicita `PrincipalForm` dentro de un ámbito.
4. **`src/StockVentas.WinForms/NavegacionFormularios.cs`:** crea un ámbito por apertura, resuelve el formulario, ejecuta `ShowDialog` y libera el ámbito al cerrarlo.
5. **Repositorios de `src/AccesoDatos/Repositorios`:** reciben `IDbContextFactory<StockVentasDbContext>` y utilizan `CreateDbContext()` por operación.
6. **`ConfiguracionBaseDatos.cs`:** comparte la configuración de SQLite entre la fábrica de ejecución y la de migraciones.
7. **Pruebas:** usan los registros reales de la biblioteca e incorporan `PruebasEtapa4.cs`.
8. **`src/AccesoDatos/Interfaces`:** contiene diez contratos, uno por servicio y por repositorio, que declaran sus operaciones públicas.

## Tiempos de vida

| Registro | Tiempo de vida | Motivo |
|---|---|---|
| Repositorios y servicios | Scoped | Se reutilizan dentro de un mismo ámbito y son diferentes en otro ámbito. |
| Formularios | Transient | Se construye una instancia nueva al solicitar una ventana. Se resuelven dentro de un ámbito para que el contenedor los libere al terminar. |
| NavegacionFormularios | Singleton | Coordina aperturas durante toda la aplicación y solo depende de la fábrica de ámbitos. |
| IDbContextFactory | Singleton | Comparte la configuración y crea contextos independientes. |
| DbContext usado por el repositorio | Por operación | El repositorio lo crea mediante la fábrica y lo libera con `using`, incluso si ocurre un error. |

`AddDbContextFactory` también permite resolver un `StockVentasDbContext` scoped directamente. Los repositorios de esta aplicación utilizan la fábrica para mantener un contexto corto por operación.

Una ventana de Productos puede abrir Proveedores. Cada apertura tiene su propio ámbito; cerrar Proveedores no libera las dependencias de Productos. El ámbito principal dura mientras se ejecuta el menú.

## Ejemplo para explicar en la exposición

Al abrir Productos, el navegador crea un ámbito y solicita `ProductosForm`. Su constructor pide `IProductoServicio`, `IProveedorServicio` y el navegador. El contenedor entrega las implementaciones registradas `ProductoServicio` y `ProveedorServicio`.

Para construir `ProductoServicio`, resuelve los contratos `IProductoRepositorio` e `IProveedorRepositorio` con sus implementaciones. Los repositorios reciben la fábrica de contextos registrada. Al guardar un producto, el repositorio crea un contexto, agrega o modifica el registro y ejecuta `SaveChanges()`.

Al terminar esa operación se libera el contexto. Al cerrar la ventana se libera su ámbito. La inyección decide cómo se obtienen las dependencias; las validaciones y la transacción de ventas siguen teniendo sus responsabilidades anteriores.

## Fábrica de ejecución y fábrica de diseño

- **`IDbContextFactory<StockVentasDbContext>`:** se registra mediante `AddDbContextFactory` y se utiliza mientras funciona la aplicación.
- **`StockVentasDbContextFactory`:** implementa `IDesignTimeDbContextFactory<StockVentasDbContext>` y permite que `dotnet ef` trabaje sin abrir WinForms.

Ambas utilizan `ConfiguracionBaseDatos.Configurar`. Se conserva la ruta por defecto y la alternativa `STOCKVENTAS_DB_PATH`.

## Paquetes y migraciones

Se agregó una referencia explícita a `Microsoft.Extensions.DependencyInjection` **8.0.1**. EF Core SQLite, Design, Tools y la herramienta local `dotnet-ef` utilizan **8.0.32**. Tools tenía una referencia a 10.0.13 en WinForms y se alineó con EF Core 8.

No se agregó una migración para la inyección, porque no cambian las entidades ni las columnas. Se conservaron `InitialCreate` y `MigracionNugget`; esta última ya existía y sus métodos `Up` y `Down` están vacíos. Las pruebas verifican que se apliquen todas las migraciones existentes, sin suponer que el historial tiene exactamente una entrada.

## Cómo comprobar la entrega

```shell
dotnet restore StockVentas.sln
dotnet tool restore
dotnet build StockVentas.sln
dotnet ef database update --project src/AccesoDatos
dotnet run --project tests/StockVentas.Pruebas
dotnet run --project src/StockVentas.WinForms
```

Las pruebas anteriores ahora pasan por el contenedor. Las nuevas comprobaciones verifican resolución de contratos e implementaciones, sustitución de un repositorio sin cambiar su servicio, instancias por ámbito, fábrica compartida con contextos independientes, errores de registros incompletos, liberación de contextos y persistencia entre ámbitos.

En Windows, abrir y cerrar dos veces cada sección del menú. Desde Productos, abrir Proveedores, cerrarlo y seguir trabajando en Productos. Luego repetir el recorrido de ventas y reportes de la Etapa 3. Esta revisión comprueba también que una ventana cerrada no se reutiliza y que cerrar una ventana secundaria no invalida la principal.

La compilación y las pruebas de datos pueden ejecutarse en macOS. La ejecución de WinForms y la navegación visual se comprueban en Windows.

## Alcance

Se mantienen los dos proyectos principales y el proyecto auxiliar de pruebas. Se conservan las categorías seleccionables, los cinco reportes, las validaciones y el registro transaccional de ventas. La etapa no incorpora autenticación ni pantalla de calificaciones.

Se agregaron interfaces propias para cada repositorio y servicio. .NET también permite inyectar clases concretas, pero los contratos permiten sustituir implementaciones y coinciden con el enfoque explicado por los profesores. Las interfaces estándar de EF Core y del contenedor se utilizan para las fábricas.

## Correspondencia con el material de clase

Material revisado: **Inyección de dependencias en .NET**, Programación I, Unidad extra, proporcionado por el grupo. Se revisaron las doce páginas y los ejemplos de código de las diapositivas 7 y 10.

| Concepto del material | Aplicación en este proyecto |
|---|---|
| Interfaz como contrato, páginas 2 a 6 | Interfaces en AccesoDatos, como `IProductoRepositorio` e `IProductoServicio`. |
| Servicio como implementación | `ProductoRepositorio : IProductoRepositorio` y `ProductoServicio : IProductoServicio`, entre otros. En DI, un repositorio también puede ser un servicio registrado. |
| Registrar contrato e implementación, páginas 4 y 7 | `AddScoped<IProductoRepositorio, ProductoRepositorio>()` y los otros nueve registros. |
| Configurar desde Program.cs, páginas 5 y 7 | `Program.cs` crea `ServiceCollection`, invoca `AgregarAplicacion()` y construye el proveedor. Los registros se agrupan en métodos para organizar el proyecto. |
| Pedir la interfaz en el formulario, página 10 | `ProductosForm(IProductoServicio productos, IProveedorServicio proveedores, NavegacionFormularios navegacion)`. |
| AddTransient, AddScoped y AddSingleton, páginas 8 y 9 | Se mantienen sus significados. El ámbito de Scoped se crea explícitamente por ventana. |

El ejemplo del material utiliza `IGenericRepository<Artista>` y `GenericRepository<Artista>`. En nuestro dominio se conservan nombres específicos: `IProductoRepositorio`, `IClienteRepositorio`, etc. Los métodos de consulta se llaman `Consultar`; cumplen la función de lectura de `ObtenerTodos` del ejemplo y admiten los filtros existentes. No se agrega un repositorio genérico ni se cambian las reglas de ventas y reportes para copiar literalmente los nombres del ejemplo.

El material señala que Scoped requiere cuidado en WinForms. Por eso se conserva `CreateScope()` en el navegador. La fábrica de EF Core es una adaptación de la gestión de contextos al sistema existente y permite mantener un contexto por operación.

## Referencias

- [Inyección de dependencias en .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/overview).
- [Configuración y fábrica de DbContext](https://learn.microsoft.com/ef/core/dbcontext-configuration/#use-a-dbcontext-factory).
