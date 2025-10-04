using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cursos
        public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            // Base query: solo cursos activos
            var query = _context.Cursos.AsQueryable().Where(c => c.Activo);

            // Filtros opcionales
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

            var cursos = await query.OrderBy(c => c.Codigo).ToListAsync();
            return View(cursos);
        }

        // GET: /Cursos/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null || !curso.Activo)
                return NotFound();

            return View(curso);
        }
    }
}
