using Algolia.Search.Clients;
using Algolia.Search.Http;
using Algolia.Search.Models.Search;

namespace IncidenciasBicicletas.Web.Services
{
    public class IncidenciaAlgoliaRecord
    {
        public string ObjectID { get; set; } = string.Empty;
        public int IncidenciaId { get; set; }
        public string Estacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class AlgoliaBusquedaService : IBusquedaService
    {
        private readonly SearchClient _client;
        private const string IndexName = "incidencias";

        public AlgoliaBusquedaService(IConfiguration configuration)
        {
            var appId = configuration["Algolia:AppId"];
            var apiKey = configuration["Algolia:WriteApiKey"];
            _client = new SearchClient(new SearchConfig(appId, apiKey));
        }

        public async Task<List<int>> BuscarIdsAsync(string termino)
        {
            var searchParams = new SearchParams(new SearchParamsObject { Query = termino });

            var respuesta = await _client.SearchSingleIndexAsync<IncidenciaAlgoliaRecord>(
                IndexName,
                searchParams
            );

            return respuesta.Hits.Select(h => h.IncidenciaId).ToList();
        }
    }
}