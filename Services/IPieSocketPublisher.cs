namespace IncidenciasBicicletas.Web.Services
{
    public interface IPieSocketPublisher
    {
        Task PublicarIncidenciaActualizadaAsync(int id, string estado);
    }
}