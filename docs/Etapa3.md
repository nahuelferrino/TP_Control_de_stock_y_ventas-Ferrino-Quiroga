# Etapa 3 — Reportes y mejoras

Fecha de entrega indicada en la consigna: **25/10/2026**.

## Cumplimiento de la consigna

| Requisito | Implementación |
|---|---|
| Al menos cinco reportes/consultas | Ventana Reportes con las cinco consultas descritas a continuación. |
| Interfaz organizada | Menú principal con Productos, Proveedores, Clientes, Ventas y Reportes; formularios con campos, acciones y listados separados. |
| Validaciones básicas | Campos obligatorios, límites de longitud, emails, categorías y proveedores seleccionables, importes, cantidades, disponibilidad y fechas. |
| Prompts utilizados en un TXT | Archivo `prompts.txt` en la raíz del repositorio, con mensajes reales de la conversación. |

## Funciones incorporadas

Se mantiene el ABM de productos y proveedores. Se agrega el ABM de clientes y el registro de ventas para que los reportes puedan usarse con datos cargados desde la aplicación.

En **Clientes** se registran nombre, apellido, teléfono y email. La baja es lógica y conserva las compras anteriores.

En **Ventas** se selecciona un cliente activo, se elige la fecha y se agregan productos con sus cantidades. El carrito permite retirar renglones y muestra el total estimado. Al confirmar, se consulta el precio actual de cada producto y se comprueba nuevamente el stock. El mensaje de confirmación muestra el total efectivamente guardado.

El registro de la venta, sus detalles y el descuento de stock se guardan dentro de una transacción. Si un producto no está disponible o no alcanza el stock, se rechaza la operación completa. Agregar productos al carrito todavía no modifica la base.

La aplicación avisa antes de descartar una venta sin guardar. Cada venta confirmada queda registrada como operación histórica; esta etapa no incorpora edición ni anulación de ventas.

## Los cinco reportes

| Reporte | Filtros | Información |
|---|---|---|
| 1. Productos con bajo stock | Ninguno: inventario actual | Código, producto, categoría, proveedor, stock disponible y mínimo. Incluye productos activos con stock menor o igual al mínimo. |
| 2. Historial de ventas por cliente | Cliente obligatorio y período | Venta, fecha, producto, cantidad, precio unitario histórico y subtotal. Incluye clientes inactivos para poder consultar sus compras anteriores. |
| 3. Ventas realizadas en un período | Fechas desde y hasta | Una fila por venta, cliente, unidades e importe. Muestra cantidad de ventas y total del período. |
| 4. Productos más vendidos | Fechas desde y hasta | Código, producto, unidades e importe vendido. Ordena por unidades de mayor a menor; los empates se ordenan por código. |
| 5. Inventario por categoría | Ninguno: inventario actual | Cantidad de productos activos, unidades y valoración del stock por categoría. Agrupa sin distinguir mayúsculas y minúsculas. |

En los reportes 2, 3 y 4 se incluye todo el día de la fecha **hasta**. Si ambas fechas coinciden, se consultan las operaciones de ese día completo. Los filtros que no aplican al reporte seleccionado quedan deshabilitados.

El historial y el ranking conservan los productos y clientes dados de baja. Los precios históricos se toman de `DetalleVenta.PrecioUnitario`, por lo que modificar el precio actual de un producto no altera los importes de ventas anteriores.

La valoración del inventario utiliza **stock actual × precio de venta actual**. No representa costo de adquisición, ganancia ni dinero cobrado. Los reportes de inventario excluyen productos inactivos y conservan productos activos aunque su proveedor haya sido dado de baja.

Cada consulta vuelve a leer SQLite. Se muestra un mensaje explícito cuando no hay resultados y se vacía el listado anterior al cambiar de reporte o generar otra consulta.

## Validaciones

- **Productos:** código, nombre y categoría obligatorios; código único incluso en productos inactivos; precio positivo con hasta dos decimales; stock disponible y mínimo no negativos; proveedor activo.
- **Proveedores:** nombre obligatorio, longitud de campos y formato del email si se completa.
- **Clientes:** nombre y apellido obligatorios, longitud de campos y formato del email si se completa.
- **Ventas:** cliente activo, al menos un producto, productos activos, cantidades enteras positivas, stock suficiente y fecha que no pertenezca a un día futuro. Los renglones del mismo producto se consolidan antes de validar el stock.
- **Reportes:** cliente obligatorio para el historial y fecha desde no posterior a fecha hasta.

## Organización del código

Se conservan los dos proyectos principales:

- `AccesoDatos`: modelos, contexto, migraciones, repositorios y servicios. Se agregan `ClienteRepositorio`, `VentaRepositorio` y `ReporteRepositorio`, sus servicios y las clases de resultados en `Reportes`.
- `StockVentas.WinForms`: menú principal y formularios. La interfaz solicita las operaciones a los servicios y no escribe directamente en las tablas.

No cambian las entidades ni las columnas de la base. Se utiliza la migración inicial existente. En una base nueva se ejecutan los mismos comandos de preparación:

```shell
dotnet restore StockVentas.sln
dotnet tool restore
dotnet ef database update --project src/AccesoDatos
dotnet run --project src/StockVentas.WinForms
```

### Operador de las ventas

El modelo existente exige asociar cada venta a un `Usuario`. Como esta etapa no incluye inicio de sesión ni administración de usuarios, la primera venta crea el registro interno `operador-local` y las siguientes lo reutilizan. Su campo `ClaveHash` se genera a partir de bytes aleatorios; no existe una contraseña predeterminada para ingresar.

Ese registro identifica las operaciones de la aplicación local y no acredita la identidad de una persona. La autenticación y las calificaciones de compras continúan pendientes.

## Recorrido de demostración

Usar una base nueva o códigos de producto nuevos para distinguir estos datos de los ya existentes.

1. Desde el menú principal, abrir **Proveedores** y registrar un proveedor.
2. En **Productos**, crear:

   | Código | Nombre | Categoría | Precio | Stock | Mínimo |
   |---|---|---|---|---|---|
   | DEMO-CUA | Cuaderno | Librería | 100,00 | 10 | 3 |
   | DEMO-LAP | Lapicera | Librería | 50,00 | 20 | 5 |

3. En **Clientes**, crear a Ana Pérez y Bruno Quiroga.
4. En **Ventas**, registrar para Ana una compra de 8 cuadernos y 2 lapiceras, con fecha de hoy. Total esperado: **900,00**.
5. Registrar para Bruno una compra de 1 cuaderno, también con fecha de hoy. Total esperado: **100,00**.
6. Abrir **Reportes** y comprobar, si la base solo tiene estos datos:

   | Consulta | Resultado esperado |
   |---|---|
   | Stock bajo | Cuaderno: stock 1, mínimo 3. |
   | Historial de Ana, incluyendo hoy | Dos renglones de una venta; total 900,00. |
   | Ventas de hoy | Dos ventas; total 1.000,00 y 11 unidades. |
   | Más vendidos de hoy | Cuaderno: 9 unidades; Lapicera: 2 unidades. |
   | Inventario por categoría | Librería: 2 productos, 19 unidades y valor de stock 1.000,00. |

7. Intentar vender 2 cuadernos adicionales: debe rechazarse porque queda 1. Confirmar que el stock y los reportes no cambiaron.
8. Cambiar el precio del cuaderno. Confirmar que los importes de ventas anteriores permanecen iguales y que la valoración del inventario usa el nuevo precio.
9. Dar de baja a Ana o al cuaderno. El historial y el ranking deben conservar sus ventas.
10. Invertir las fechas desde y hasta, o consultar el historial sin seleccionar cliente: debe aparecer un mensaje y no debe quedar visible un resultado anterior.

Si la base ya contiene movimientos, los reportes pueden mostrar otros registros además de los de demostración. La aplicación no agrega ventas ficticias ni borra información existente.

## Verificación

```shell
dotnet build StockVentas.sln
dotnet run --project tests/StockVentas.Pruebas
```

Las pruebas de la Etapa 3 usan otra base temporal, separada de la base del usuario. Comprueban clientes, registro de ventas, descuentos de stock, rechazo de operaciones incompletas, persistencia, los cinco reportes, límites de fechas, precios históricos y el caso de dos ventas que intentan consumir la misma última unidad.

La compilación y las pruebas de base de datos pueden ejecutarse en macOS. La ejecución y revisión visual de los formularios requieren Windows.
