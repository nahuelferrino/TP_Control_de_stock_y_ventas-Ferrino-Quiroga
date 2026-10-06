# TP\_Control\_de\_stock\_y\_ventas-Ferrino-Quiroga



#### **Sistema de Gestión de Stock y Ventas**



##### **Descripción**

**Aplicación de escritorio desarrollada en Windows Forms con conexión a base de datos mediante Entity Framework Core.**  

**El sistema permite gestionar el stock de productos y las operaciones de ventas, organizando la información en capas con una biblioteca de clases.**  



##### **Entidades principales**

**- Producto: código, nombre, categoría, precio, stock disponible.**  

**- Cliente: datos personales y de contacto.**  

**- Venta: fecha, cliente, detalle de productos vendidos, total.**  

**- Usuario: credenciales para acceso al sistema.**  



**---**



##### &#x20;**Objetivos y Funcionalidades**

**El sistema busca:**

**- Organizar y simplificar la gestión de stock y ventas.**  

**- Permitir operaciones CRUD (Altas, Bajas, Modificaciones, Consultas) sobre las entidades principales.**  

**- Generar reportes básicos para análisis y control.**  



##### &#x20;**Funcionalidades CRUD**

**1. Productos**  

&#x20;  **- Alta de nuevos productos con stock inicial.**  

&#x20;  **- Baja de productos obsoletos o discontinuados.**  

&#x20;  **- Modificación de precios y stock.**  



**2. Clientes**  

&#x20;  **- Alta de clientes nuevos.**  

&#x20;  **- Baja de clientes inactivos.**  

&#x20;  **- Modificación de datos de contacto.**  



**---**



##### &#x20;**Reportes previstos**

**El sistema incluirá al menos 4 reportes básicos:**



**1. Listado de productos con bajo stock (alerta de reposición).**  

**2. Historial de ventas por cliente (detalle de compras realizadas).**  

**3. Ventas realizadas en un período (filtrado por fechas).**  

**4. \*Productos más vendidos (ranking de popularidad).**  



**---**



##### &#x20;**Integración de capas para guardar un registro**

**El sistema está organizado en capas siguiendo buenas prácticas de arquitectura:**



**1. Capa de Presentación (Windows Forms)** 

&#x20;  **- El usuario completa un formulario (ejemplo: alta de producto).**  

&#x20;  **- Se valida la información ingresada.**  



**2. Capa de Lógica de Negocio**

&#x20;  **- Se aplican reglas de negocio (ejemplo: no permitir stock negativo).**  

&#x20;  **- Se construye el objeto `Producto` con los datos recibidos.**  



**3. Capa de Acceso a Datos (Repositorios con Entity Framework Core)** 

&#x20;  **- El repositorio recibe el objeto y lo agrega al contexto (`DbContext`).**  

&#x20;  **- Se ejecuta `SaveChanges()` para persistir el registro en la base de datos SQLite.**  



**4. Base de Datos (SQLite)** 

&#x20;  **- El registro queda almacenado en la tabla correspondiente.**  

&#x20;  **- La información puede ser consultada** 

**---**



