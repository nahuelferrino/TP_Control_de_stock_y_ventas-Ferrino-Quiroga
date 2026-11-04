using AccesoDatos.Modelos;

namespace AccesoDatos.Interfaces;

public interface IProductoServicio
{
    List<Producto> Consultar(string busqueda = "", bool incluirInactivos = false);
    List<string> ConsultarCategorias();
    void Guardar(Producto producto);
    void DarDeBaja(int id);
}
