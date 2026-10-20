using System.Net.Mail;
using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;

namespace AccesoDatos.Servicios;

public class ClienteServicio
{
    private readonly ClienteRepositorio clientes;

    public ClienteServicio(ClienteRepositorio clientes) => this.clientes = clientes;

    public List<Cliente> Consultar(bool incluirInactivos = false) => clientes.Consultar(incluirInactivos);

    public void Guardar(Cliente cliente)
    {
        cliente.Nombre = cliente.Nombre.Trim();
        cliente.Apellido = cliente.Apellido.Trim();
        cliente.Telefono = cliente.Telefono.Trim();
        cliente.Email = cliente.Email.Trim();
        if (cliente.Nombre.Length is 0 or > 100 || cliente.Apellido.Length is 0 or > 100)
            throw new ArgumentException("El nombre y el apellido son obligatorios y admiten hasta 100 caracteres cada uno.");
        if (cliente.Telefono.Length > 30 || cliente.Email.Length > 150)
            throw new ArgumentException("El teléfono admite hasta 30 caracteres y el email hasta 150.");
        if (cliente.Email.Length > 0
            && (!MailAddress.TryCreate(cliente.Email, out var email) || email.Address != cliente.Email))
            throw new ArgumentException("El email del cliente no tiene un formato válido.");
        if (cliente.Id == 0)
        {
            cliente.Activo = true;
            clientes.Agregar(cliente);
        }
        else clientes.Modificar(cliente);
    }

    public void DarDeBaja(int id) => clientes.DarDeBaja(id);
}
