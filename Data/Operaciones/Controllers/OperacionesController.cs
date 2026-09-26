using IncidenciasBicicletas.Web.Data;
using IncidenciasBicicletas.Web.Models;
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

        public IncidenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Operaciones/Incidencias
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

        // POST: /Operaciones/Incidencias/Cerrar/5
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

            TempData["Exito"] = $"Incidencia #{incidencia.Id} cerrada.";
            return RedirectToAction(nameof(Index));
        }
    }
}