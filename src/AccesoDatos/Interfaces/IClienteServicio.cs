using AccesoDatos.Modelos;

namespace AccesoDatos.Interfaces;

public interface IClienteServicio
{
    List<Cliente> Consultar(bool incluirInactivos = false);
    void Guardar(Cliente cliente);
    void DarDeBaja(int id);
}
