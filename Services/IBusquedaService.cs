namespace IncidenciasBicicletas.Web.Services
{
    public interface IBusquedaService
    {
        Task<List<int>> BuscarIdsAsync(string termino);
    }
}