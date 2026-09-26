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
        private readonly ICacheService _cacheService;
        private const string CacheKeyListado = "incidencias:listado:abiertas";

        public IncidenciasController(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        // GET: /Operaciones/Incidencias
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var abiertas = await _cacheService.GetAsync<List<Incidencia>>(CacheKeyListado);

            if (abiertas == null)
            {
                abiertas = await _context.Incidencias
                    .Where(i => i.Estado == EstadoIncidencia.Abierta)
                    .OrderByDescending(i => i.Prioridad)
                    .ThenByDescending(i => i.FechaRegistro)
                    .ToListAsync();

                await _cacheService.SetAsync(CacheKeyListado, abiertas, TimeSpan.FromSeconds(60));
            }

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

            // Invalidar el cache ANTES de volver a consultarlo (regla del enunciado)
            await _cacheService.RemoveAsync(CacheKeyListado);

            TempData["Exito"] = $"Incidencia #{incidencia.Id} cerrada.";
            return RedirectToAction(nameof(Index));
        }
    }
}