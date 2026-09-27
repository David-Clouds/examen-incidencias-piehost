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
        private readonly ICacheService _cacheService;
        private readonly IPieSocketPublisher _publisher;
        private const string CacheKeyListado = "incidencias:listado:abiertas";

        public IncidenciasController(
            ApplicationDbContext context,
            IBusquedaService busquedaService,
            ICacheService cacheService,
            IPieSocketPublisher publisher)
        {
            _context = context;
            _busquedaService = busquedaService;
            _cacheService = cacheService;
            _publisher = publisher;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? termino)
        {
            List<Incidencia> abiertas;

            if (!string.IsNullOrWhiteSpace(termino))
            {
                // Búsqueda con Algolia: consulta DIRECTA, sin usar la caché (regla del enunciado)
                var idsEncontrados = await _busquedaService.BuscarIdsAsync(termino);

                abiertas = await _context.Incidencias
                    .Where(i => idsEncontrados.Contains(i.Id) && i.Estado == EstadoIncidencia.Abierta)
                    .OrderByDescending(i => i.Prioridad)
                    .ThenByDescending(i => i.FechaRegistro)
                    .ToListAsync();
            }
            else
            {
                // Listado general: usa caché Redis (60s)
                var cacheado = await _cacheService.GetAsync<List<Incidencia>>(CacheKeyListado);

                if (cacheado == null)
                {
                    cacheado = await _context.Incidencias
                        .Where(i => i.Estado == EstadoIncidencia.Abierta)
                        .OrderByDescending(i => i.Prioridad)
                        .ThenByDescending(i => i.FechaRegistro)
                        .ToListAsync();

                    await _cacheService.SetAsync(CacheKeyListado, cacheado, TimeSpan.FromSeconds(60));
                }

                abiertas = cacheado;
            }

            ViewBag.Termino = termino;
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

            // 1. Persistir el estado primero
            incidencia.Estado = EstadoIncidencia.Cerrada;
            await _context.SaveChangesAsync();

            // 2. Invalidar la caché de Redis
            await _cacheService.RemoveAsync(CacheKeyListado);

            // 3. Publicar el evento por PieSocket (después de persistir e invalidar)
            await _publisher.PublicarIncidenciaActualizadaAsync(incidencia.Id, incidencia.Estado.ToString());

            TempData["Exito"] = $"Incidencia #{incidencia.Id} cerrada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Operaciones/Incidencias/Estado/5 (para reconectar y consultar estado vigente)
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