using AccesoDatos.Modelos;

namespace AccesoDatos.Interfaces;

public interface IProductoRepositorio
{
    List<Producto> Consultar(string busqueda = "", bool incluirInactivos = false);
    Producto? ObtenerPorId(int id);
    List<string> ConsultarCategorias();
    bool ExisteCodigo(string codigo, int excluirId = 0);
    void Agregar(Producto producto);
    void Modificar(Producto producto);
    void DarDeBaja(int id);
}
