using System.ComponentModel.DataAnnotations;

namespace IncidenciasBicicletas.Web.Models
{
    public enum PrioridadIncidencia
    {
        Baja,
        Media,
        Alta
    }

    public enum EstadoIncidencia
    {
        Abierta,
        Cerrada
    }

    public class Incidencia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La estación es obligatoria")]
        [Display(Name = "Estación")]
        public string Estacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        public PrioridadIncidencia Prioridad { get; set; } = PrioridadIncidencia.Media;

        public EstadoIncidencia Estado { get; set; } = EstadoIncidencia.Abierta;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}