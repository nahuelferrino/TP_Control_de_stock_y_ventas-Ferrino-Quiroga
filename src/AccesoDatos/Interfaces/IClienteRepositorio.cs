using AccesoDatos.Modelos;

namespace AccesoDatos.Interfaces;

public interface IClienteRepositorio
{
    List<Cliente> Consultar(bool incluirInactivos = false);
    void Agregar(Cliente cliente);
    void Modificar(Cliente cliente);
    void DarDeBaja(int id);
}
