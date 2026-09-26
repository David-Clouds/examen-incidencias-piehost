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
        private readonly IBusquedaService _busquedaService;

        public IncidenciasController(ApplicationDbContext context, IBusquedaService busquedaService)
        {
            _context = context;
            _busquedaService = busquedaService;
        }

        // GET: /Operaciones/Incidencias
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? termino)
        {
            List<Incidencia> abiertas;

            if (!string.IsNullOrWhiteSpace(termino))
            {
                var idsEncontrados = await _busquedaService.BuscarIdsAsync(termino);

                abiertas = await _context.Incidencias
                    .Where(i => idsEncontrados.Contains(i.Id) && i.Estado == EstadoIncidencia.Abierta)
                    .OrderByDescending(i => i.Prioridad)
                    .ThenByDescending(i => i.FechaRegistro)
                    .ToListAsync();
            }
            else
            {
                abiertas = await _context.Incidencias
                    .Where(i => i.Estado == EstadoIncidencia.Abierta)
                    .OrderByDescending(i => i.Prioridad)
                    .ThenByDescending(i => i.FechaRegistro)
                    .ToListAsync();
            }

            ViewBag.Termino = termino;
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