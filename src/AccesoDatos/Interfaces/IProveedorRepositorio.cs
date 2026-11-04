using AccesoDatos.Modelos;

namespace AccesoDatos.Interfaces;

public interface IProveedorRepositorio
{
    List<Proveedor> Consultar(bool incluirInactivos = false);
    Proveedor? ObtenerPorId(int id);
    void Agregar(Proveedor proveedor);
    void Modificar(Proveedor proveedor);
    void DarDeBaja(int id);
}
