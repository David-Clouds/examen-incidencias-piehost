using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace IncidenciasBicicletas.Web.Services
{
    public class PieSocketPublisher : IPieSocketPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PieSocketPublisher> _logger;

        public PieSocketPublisher(IConfiguration configuration, ILogger<PieSocketPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task PublicarIncidenciaActualizadaAsync(int id, string estado)
        {
            var clusterId = _configuration["PieSocket:ClusterId"];
            var apiKey = _configuration["PieSocket:ApiKey"];
            var canal = _configuration["PieSocket:Canal"];

            var url = $"wss://{clusterId}.piesocket.com/v3/{canal}?api_key={apiKey}&notify_self=1";

            using var socket = new ClientWebSocket();

            try
            {
                await socket.ConnectAsync(new Uri(url), CancellationToken.None);

                var evento = new
                {
                    tipo = "IncidenciaActualizada",
                    id,
                    estado
                };

                var mensaje = JsonSerializer.Serialize(evento);
                var bytes = Encoding.UTF8.GetBytes(mensaje);

                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Publicado", CancellationToken.None);

                _logger.LogInformation("Evento IncidenciaActualizada publicado en PieSocket: Id={Id}, Estado={Estado}", id, estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar evento en PieSocket");
            }
        }
    }
}