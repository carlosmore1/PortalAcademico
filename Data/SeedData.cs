using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public static class SeedData
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            const string rolCoordinador = "Coordinador";

            // Crear rol
            if (!await roleManager.RoleExistsAsync(rolCoordinador))
                await roleManager.CreateAsync(new IdentityRole(rolCoordinador));

            // Crear usuario coordinador
            var email = "coordinador@demo.com";
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, "P@ssw0rd!");
                await userManager.AddToRoleAsync(user, rolCoordinador);
            }

            // Cursos iniciales
            if (!await context.Cursos.AnyAsync())
            {
                var cursos = new[]
                {
                    new Curso
                    {
                        Codigo = "INF101",
                        Nombre = "Introducción a la Programación",
                        Creditos = 3,
                        CupoMaximo = 30,
                        HorarioInicio = new TimeSpan(8,0,0),
                        HorarioFin = new TimeSpan(10,0,0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "BD201",
                        Nombre = "Bases de Datos",
                        Creditos = 4,
                        CupoMaximo = 25,
                        HorarioInicio = new TimeSpan(10,0,0),
                        HorarioFin = new TimeSpan(12,0,0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "WEB301",
                        Nombre = "Desarrollo Web",
                        Creditos = 4,
                        CupoMaximo = 20,
                        HorarioInicio = new TimeSpan(14,0,0),
                        HorarioFin = new TimeSpan(16,0,0),
                        Activo = true
                    }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}
