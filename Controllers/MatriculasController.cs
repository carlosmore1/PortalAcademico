using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize] // solo usuarios autenticados pueden inscribirse
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Matriculas/Crear?cursoId=5
        public async Task<IActionResult> Crear(int cursoId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
                return NotFound();

            ViewBag.Curso = curso;
            return View();
        }

        // POST: /Matriculas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearConfirmado(int cursoId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == cursoId);

            if (curso == null || !curso.Activo)
            {
                TempData["Error"] = "El curso no existe o no está activo.";
                return RedirectToAction("Index", "Cursos");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Debe iniciar sesión para inscribirse.";
                return RedirectToAction("Login", "Account");
            }

            // Validación 1: no duplicar matrícula
            var yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.CursoId == curso.Id && m.UsuarioId == user.Id);

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya está matriculado en este curso.";
                return RedirectToAction("Detalle", "Cursos", new { id = curso.Id });
            }

            // Validación 2: no exceder el cupo
            var cupoActual = await _context.Matriculas
                .CountAsync(m => m.CursoId == curso.Id && m.Estado != EstadoMatricula.Cancelada);

            if (cupoActual >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ya alcanzó su cupo máximo.";
                return RedirectToAction("Detalle", "Cursos", new { id = curso.Id });
            }

            // Validación 3: no solapar horarios
            var misMatriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == user.Id && m.Estado != EstadoMatricula.Cancelada)
                .ToListAsync();

            bool solapa = misMatriculas.Any(m =>
                (curso.HorarioInicio < m.Curso.HorarioFin) &&
                (m.Curso.HorarioInicio < curso.HorarioFin));

            if (solapa)
            {
                TempData["Error"] = "El horario de este curso se solapa con otro ya matriculado.";
                return RedirectToAction("Detalle", "Cursos", new { id = curso.Id });
            }

            // Si todo pasa → registrar matrícula
            var matricula = new Matricula
            {
                CursoId = curso.Id,
                UsuarioId = user.Id,
                Estado = EstadoMatricula.Pendiente,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Se registró su matrícula en {curso.Nombre}.";
            return RedirectToAction("Detalle", "Cursos", new { id = curso.Id });
        }
    }
}
