# TP_Control_de_stock_y_ventas-Ferrino-Quiroga

## Sistema de Gestión de Stock y Ventas

### Descripción

Aplicación de escritorio desarrollada en Windows Forms con conexión a base de datos mediante Entity Framework Core.

El sistema permite gestionar el stock de productos y las operaciones de ventas, organizando la información en capas con una biblioteca de clases.

### Entidades principales

- **Producto:** código, nombre, categoría, precio, stock disponible.
- **Cliente:** datos personales y de contacto.
- **Venta:** fecha, cliente, detalle de productos vendidos, total.
- **Usuario:** credenciales para acceso al sistema.

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

**2. Clientes**

- Alta de clientes nuevos.
- Baja de clientes inactivos.
- Modificación de datos de contacto.

---

### Reportes previstos

El sistema incluirá al menos 4 reportes básicos:

1. Listado de productos con bajo stock (alerta de reposición).
2. Historial de ventas por cliente (detalle de compras realizadas).
3. Ventas realizadas en un período (filtrado por fechas).
4. Productos más vendidos (ranking de popularidad).

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
