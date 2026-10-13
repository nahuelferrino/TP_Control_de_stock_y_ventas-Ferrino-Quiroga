using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;

namespace AccesoDatos.Servicios;

public class ProductoServicio
{
    private readonly ProductoRepositorio productos;
    private readonly ProveedorRepositorio proveedores;

    public ProductoServicio(ProductoRepositorio productos, ProveedorRepositorio proveedores)
    {
        this.productos = productos;
        this.proveedores = proveedores;
    }

    public List<Producto> Consultar(string busqueda = "", bool incluirInactivos = false)
        => productos.Consultar(busqueda, incluirInactivos);

    public void Guardar(Producto producto)
    {
        producto.Codigo = producto.Codigo.Trim().ToUpperInvariant();
        producto.Nombre = producto.Nombre.Trim();
        producto.Categoria = producto.Categoria.Trim();
        if (producto.Codigo.Length is 0 or > 30)
            throw new ArgumentException("El código es obligatorio y debe tener hasta 30 caracteres.");
        if (producto.Nombre.Length is 0 or > 100)
            throw new ArgumentException("El nombre es obligatorio y debe tener hasta 100 caracteres.");
        if (producto.Categoria.Length is 0 or > 60)
            throw new ArgumentException("La categoría es obligatoria y debe tener hasta 60 caracteres.");
        if (producto.Precio <= 0 || producto.Precio > 999999999.99m
            || decimal.Round(producto.Precio, 2) != producto.Precio)
            throw new ArgumentException("El precio debe ser mayor que cero, tener hasta dos decimales y no superar 999.999.999,99.");
        if (producto.StockDisponible < 0 || producto.StockMinimo < 0)
            throw new ArgumentException("El stock disponible y el stock mínimo no pueden ser negativos.");
        if (productos.ExisteCodigo(producto.Codigo, producto.Id))
            throw new ArgumentException("Ya existe un producto con ese código, incluso si está dado de baja.");
        var proveedor = proveedores.ObtenerPorId(producto.ProveedorId);
        if (proveedor is null || !proveedor.Activo)
            throw new ArgumentException("Seleccioná un proveedor activo. Podés registrarlo desde el botón Proveedores.");

        if (producto.Id == 0)
        {
            producto.Activo = true;
            productos.Agregar(producto);
        }
        else
            productos.Modificar(producto);
    }

    public void DarDeBaja(int id) => productos.DarDeBaja(id);
}
