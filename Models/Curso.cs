using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required, MaxLength(16)]
        public string Codigo { get; set; } = default!; // único

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = default!;

        [Range(1, int.MaxValue)]
        public int Creditos { get; set; }  // > 0

        [Range(1, int.MaxValue)]
        public int CupoMaximo { get; set; } // > 0

        // Usamos TimeSpan para horas HH:mm
        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}
