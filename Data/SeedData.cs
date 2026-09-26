using IncidenciasBicicletas.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IncidenciasBicicletas.Web.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Supervisor"))
            {
                await roleManager.CreateAsync(new IdentityRole("Supervisor"));
            }

            var supervisor = await userManager.FindByEmailAsync("supervisor@incidencias.com");
            if (supervisor == null)
            {
                supervisor = new IdentityUser
                {
                    UserName = "supervisor@incidencias.com",
                    Email = "supervisor@incidencias.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(supervisor, "Supervisor123!");
                await userManager.AddToRoleAsync(supervisor, "Supervisor");
            }

            if (!context.Incidencias.Any())
            {
                context.Incidencias.AddRange(
                    new Incidencia { Estacion = "Estación Miraflores", Descripcion = "Freno delantero dañado", Prioridad = PrioridadIncidencia.Alta, Estado = EstadoIncidencia.Abierta, FechaRegistro = DateTime.UtcNow.AddHours(-3) },
                    new Incidencia { Estacion = "Estación Surco", Descripcion = "Llanta trasera pinchada", Prioridad = PrioridadIncidencia.Media, Estado = EstadoIncidencia.Abierta, FechaRegistro = DateTime.UtcNow.AddHours(-1) },
                    new Incidencia { Estacion = "Estación San Isidro", Descripcion = "Cadena suelta", Prioridad = PrioridadIncidencia.Baja, Estado = EstadoIncidencia.Cerrada, FechaRegistro = DateTime.UtcNow.AddDays(-1) },
                    new Incidencia { Estacion = "Estación Barranco", Descripcion = "Candado electrónico no responde", Prioridad = PrioridadIncidencia.Alta, Estado = EstadoIncidencia.Abierta, FechaRegistro = DateTime.UtcNow.AddMinutes(-30) }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}