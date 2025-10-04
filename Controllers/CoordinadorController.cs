using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Coordinador
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos.OrderBy(c => c.Codigo).ToListAsync();
            return View(cursos);
        }

        // GET: /Coordinador/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Coordinador/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Curso curso)
        {
            if (!ModelState.IsValid)
                return View(curso);

            _context.Add(curso);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Curso creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Coordinador/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            return View(curso);
        }

        // POST: /Coordinador/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Curso curso)
        {
            if (id != curso.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(curso);

            _context.Update(curso);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Curso actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Coordinador/Desactivar/5
        public async Task<IActionResult> Desactivar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            curso.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Curso desactivado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Coordinador/Matriculas/5
        public async Task<IActionResult> Matriculas(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .ThenInclude(m => m.Curso)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            return View(curso);
        }

        // POST: /Coordinador/CambiarEstadoMatricula
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMatricula(int matriculaId, EstadoMatricula nuevoEstado)
        {
            var matricula = await _context.Matriculas.FindAsync(matriculaId);
            if (matricula == null)
                return NotFound();

            matricula.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Matrícula {nuevoEstado}.";
            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }
    }
}
