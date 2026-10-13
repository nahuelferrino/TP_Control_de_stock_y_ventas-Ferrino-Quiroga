using System.Net.Mail;
using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;

namespace AccesoDatos.Servicios;

public class ProveedorServicio
{
    private readonly ProveedorRepositorio proveedores;

    public ProveedorServicio(ProveedorRepositorio proveedores) => this.proveedores = proveedores;

    public List<Proveedor> Consultar(bool incluirInactivos = false)
        => proveedores.Consultar(incluirInactivos);

    public void Guardar(Proveedor proveedor)
    {
        proveedor.Nombre = proveedor.Nombre.Trim();
        proveedor.Telefono = proveedor.Telefono.Trim();
        proveedor.Email = proveedor.Email.Trim();
        proveedor.Direccion = proveedor.Direccion.Trim();
        if (proveedor.Nombre.Length is 0 or > 100)
            throw new ArgumentException("El nombre del proveedor es obligatorio y debe tener hasta 100 caracteres.");
        if (proveedor.Telefono.Length > 30 || proveedor.Email.Length > 150 || proveedor.Direccion.Length > 200)
            throw new ArgumentException("Revisá la longitud del teléfono, email o dirección.");
        if (proveedor.Email.Length > 0
            && (!MailAddress.TryCreate(proveedor.Email, out var email) || email.Address != proveedor.Email))
            throw new ArgumentException("El email del proveedor no tiene un formato válido.");

        if (proveedor.Id == 0)
        {
            proveedor.Activo = true;
            proveedores.Agregar(proveedor);
        }
        else
            proveedores.Modificar(proveedor);
    }

    public void DarDeBaja(int id) => proveedores.DarDeBaja(id);
}
