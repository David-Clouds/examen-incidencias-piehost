using IncidenciasBicicletas.Web.Data;
using IncidenciasBicicletas.Web.Models;
using IncidenciasBicicletas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IncidenciasBicicletas.Web.Controllers
{
    [Authorize]
    [Route("Operaciones/[controller]")]
    public class IncidenciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPieSocketPublisher _publisher;

        public IncidenciasController(ApplicationDbContext context, IPieSocketPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var abiertas = await _context.Incidencias
                .Where(i => i.Estado == EstadoIncidencia.Abierta)
                .OrderByDescending(i => i.Prioridad)
                .ThenByDescending(i => i.FechaRegistro)
                .ToListAsync();

            return View(abiertas);
        }

        [HttpPost("Cerrar/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Cerrar(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);

            if (incidencia == null)
                return NotFound();

            incidencia.Estado = EstadoIncidencia.Cerrada;
            await _context.SaveChangesAsync();

            await _publisher.PublicarIncidenciaActualizadaAsync(incidencia.Id, incidencia.Estado.ToString());

            TempData["Exito"] = $"Incidencia #{incidencia.Id} cerrada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Estado/{id:int}")]
        public async Task<IActionResult> Estado(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia == null)
                return NotFound();

            return Json(new { id = incidencia.Id, estado = incidencia.Estado.ToString() });
        }
    }
}