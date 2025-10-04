using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        public Curso Curso { get; set; } = default!;

        [Required]
        public string UsuarioId { get; set; } = default!; // IdentityUser key (string)

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }
}
