using AccesoDatos.Modelos;

namespace AccesoDatos.Interfaces;

public interface IProveedorServicio
{
    List<Proveedor> Consultar(bool incluirInactivos = false);
    void Guardar(Proveedor proveedor);
    void DarDeBaja(int id);
}
