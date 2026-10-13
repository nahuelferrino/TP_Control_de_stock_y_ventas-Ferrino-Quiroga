# Etapa 2 — Prototipo de Stock y Ventas

## Alcance

El prototipo implementa el ABM de productos y una ventana de proveedores para poder asignar un proveedor principal a cada producto. Las bajas son lógicas: el registro queda con `Activo = false` y puede consultarse mediante **Mostrar inactivos**.

Se incluyen los siete modelos del diagrama: `Producto`, `Proveedor`, `Cliente`, `Usuario`, `Venta`, `DetalleVenta` y `Calificacion`. En esta etapa, los últimos cinco participan del esquema de la base, pero solo `Producto` y `Proveedor` tienen pantallas de ABM. No se implementaron todavía ventas, inicio de sesión, calificaciones ni reportes.

## Proyectos y responsabilidades

| Ubicación | Contenido |
|---|---|
| `src/StockVentas.Datos/Modelos` | Entidades y propiedades de navegación. Cliente, Usuario y Proveedor son independientes. |
| `src/StockVentas.Datos/Contexto` | `DbContext`, configuración de SQLite y fábrica para las herramientas de migraciones. |
| `src/StockVentas.Datos/Repositorios` | Consultas LINQ, altas, modificaciones y bajas mediante `SaveChanges()`. |
| `src/StockVentas.Datos/Servicios` | Validaciones y reglas de negocio. |
| `src/StockVentas.Datos/Migrations` | Migración inicial y snapshot generados por EF Core. |
| `src/StockVentas.WinForms` | Inicio de la aplicación y formularios escritos en C#. |
| `tests/StockVentas.Pruebas` | Programa de pruebas de integración con SQLite real. Es un proyecto auxiliar, separado de los dos proyectos de la aplicación. |

**Recorrido de guardado:** formulario → servicio → repositorio → contexto de EF Core → SQLite.

Cada operación del repositorio utiliza un contexto nuevo y lo libera con `using`. Así, los listados vuelven a consultar los datos guardados y no dependen de una colección en memoria.

## Preparación en Windows con VS Code

1. Instalar el **SDK de .NET 8**, disponible en la [página oficial de Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0). Se necesita el SDK, no solo el runtime.
2. Abrir la carpeta del repositorio en VS Code. La extensión recomendada es **C# Dev Kit**.
3. En la terminal integrada, ejecutar:

```shell
dotnet restore StockVentas.sln
dotnet tool restore
dotnet build StockVentas.sln
dotnet ef database update --project src/StockVentas.Datos
dotnet run --project src/StockVentas.WinForms
```

`Ctrl+Shift+B` compila la solución. `F5` inicia el formulario con la configuración de depuración incluida. En **Terminal → Ejecutar tarea** también están disponibles las tareas para actualizar la base y ejecutar las pruebas.

Los formularios se construyen con controles y contenedores de WinForms desde código C#; no requieren el diseñador de Visual Studio.

## Base de datos y migraciones

Se mantiene SQLite, que fue la base elegida en la Etapa 1. La Unidad 9 muestra SQL Server, pero se utiliza el mismo procedimiento de EF Core: modelos, `DbContext`, generación de migraciones y actualización de la base. El proveedor instalado aquí es `Microsoft.EntityFrameworkCore.Sqlite`.

La herramienta local `dotnet-ef` y los paquetes de EF Core utilizan la misma versión, **8.0.32**. El material muestra una versión anterior de la herramienta; el proyecto utiliza EF Core 8 para acompañar .NET 8.

La migración **InitialCreate ya está incluida**. Para crear la base de un equipo nuevo solo se necesita:

```shell
dotnet tool restore
dotnet ef database update --project src/StockVentas.Datos
```

Para un cambio posterior en los modelos:

```shell
dotnet ef migrations add NombreDelCambio --project src/StockVentas.Datos
dotnet ef database update --project src/StockVentas.Datos
```

No se debe crear nuevamente `InitialCreate`. Se versionan la migración, su archivo de metadatos y el snapshot. La aplicación también ejecuta `Database.Migrate()` al iniciar para aplicar las migraciones pendientes. No utiliza `EnsureCreated()`, ya que la creación y evolución del esquema se realiza mediante migraciones.

La fábrica `StockVentasDbContextFactory` permite ejecutar estos comandos usando la biblioteca de clases, sin iniciar WinForms.

### Ruta del archivo

Por defecto, en Windows la base está en:

```text
%LOCALAPPDATA%\StockVentas\stockventas.db
```

La aplicación y la herramienta de migraciones comparten esa configuración. Para usar un archivo distinto, definir `STOCKVENTAS_DB_PATH` en la terminal desde la que se ejecutan ambas operaciones. Por ejemplo, en PowerShell:

```powershell
$env:STOCKVENTAS_DB_PATH = Join-Path $PWD "datos/stockventas.db"
dotnet ef database update --project src/StockVentas.Datos
dotnet run --project src/StockVentas.WinForms
```

La carpeta se crea automáticamente. El archivo persiste aunque se cierre la aplicación, y los archivos SQLite se excluyen del control de versiones. No hay datos de ejemplo precargados: los registros iniciales se crean desde los formularios.

## Recorrido de demostración

1. Abrir la aplicación y presionar **Proveedores**.
2. Crear un proveedor con nombre y, si se desea, teléfono, email y dirección.
3. Cerrar esa ventana. En productos, ingresar código, nombre, categoría, proveedor, precio, stock y stock mínimo. Presionar **Guardar**.
4. Consultar el listado y buscar por código, nombre o categoría.
5. Seleccionar el producto, modificar precio o stock y guardar. Verificar el cambio en el listado.
6. Cerrar completamente la aplicación y volver a abrirla. Confirmar que el producto sigue guardado.
7. Seleccionar el producto y presionar **Dar de baja**. Confirmar la operación.
8. Marcar **Mostrar inactivos**. El producto debe aparecer como inactivo y no admitir modificaciones.
9. Intentar guardar un producto con código repetido, precio cero o sin proveedor: debe mostrarse un mensaje y no guardarse.

El stock y el stock mínimo no admiten valores negativos. Los productos con stock menor o igual al mínimo se muestran en amarillo.

Si se da de baja a un proveedor, sus productos permanecen. Para guardar cambios en uno de esos productos se debe seleccionar un proveedor activo.

## Pruebas de integración

```shell
dotnet run --project tests/StockVentas.Pruebas
```

Las pruebas crean un archivo SQLite temporal, aplican la migración y comprueban altas, consultas, modificaciones, bajas, validaciones, restricciones de la base y persistencia al abrir nuevos contextos. También comprueban las relaciones de ventas, precios históricos y calificaciones. El programa devuelve un código distinto de cero si alguna comprobación falla.

Las pruebas eliminan únicamente su propia carpeta temporal al finalizar. No utilizan ni modifican la base del usuario.

## Trabajo desde macOS

Con el SDK de .NET 8 se puede compilar la solución, actualizar la base y ejecutar las pruebas de integración. El proyecto WinForms tiene `EnableWindowsTargeting` para permitir su compilación desde macOS. La ejecución y revisión visual de los formularios debe hacerse en Windows, porque WinForms es una tecnología de escritorio de Windows.

## Referencias

- [Unidad 9 — SQL y Entity Framework](https://docs.google.com/presentation/d/19bocjZ6iYPQnWwUTwuCWZ2nmEFNpi4S3/edit).
- [Migraciones de EF Core](https://learn.microsoft.com/ef/core/managing-schemas/migrations/).
- [Windows Forms](https://learn.microsoft.com/dotnet/desktop/winforms/overview/).
