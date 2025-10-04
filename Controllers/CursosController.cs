using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Text.Json;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Cursos
        public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            // 🔹 1) Traer lista de cursos activos desde caché (Redis)
            const string cacheKey = "cursos_activos";
            List<Curso>? cursos;

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                cursos = JsonSerializer.Deserialize<List<Curso>>(cached);
            }
            else
            {
                cursos = await _context.Cursos
                    .Where(c => c.Activo)
                    .OrderBy(c => c.Codigo)
                    .ToListAsync();

                var json = JsonSerializer.Serialize(cursos);
                await _cache.SetStringAsync(
                    cacheKey,
                    json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                    });
            }

            // 🔹 2) Aplicar filtros en memoria (sobre la lista cacheada)
            var query = cursos!.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(c => c.Nombre.Contains(nombre));

            if (creditosMin.HasValue)
                query = query.Where(c => c.Creditos >= creditosMin.Value);

            if (creditosMax.HasValue)
                query = query.Where(c => c.Creditos <= creditosMax.Value);

            if (horaInicio.HasValue)
                query = query.Where(c => c.HorarioInicio >= horaInicio.Value);

            if (horaFin.HasValue)
                query = query.Where(c => c.HorarioFin <= horaFin.Value);

            return View(query.ToList());
        }

        // GET: /Cursos/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null || !curso.Activo)
                return NotFound();

            // 🔹 Guardar en sesión el último curso visitado
            HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);
            HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

            return View(curso);
        }

        // 🔹 Helper para invalidar caché (úsalo al crear/editar cursos en P5)
        private async Task InvalidarCacheCursos()
        {
            await _cache.RemoveAsync("cursos_activos");
        }
    }
}
