using ElaraMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Data;

public static class SeedData
{
    public static readonly string[] Roles = { "Administrador", "Recepcionista", "Especialista" };

    public static readonly string[] EspecialidadesBase =
    {
        // Uñas
        "Manicura clásica", "Pedicura", "Uñas acrílicas", "Uñas en gel",
        "Nail art", "Uñas esculpidas", "Baño de escarcha", "Diseño 3D",
        // Cejas
        "Diseño de cejas", "Laminado de cejas", "Henna de cejas",
        // Pestañas
        "Extensiones de pestañas", "Lifting de pestañas",
        // Depilación
        "Depilación con cera", "Depilación con hilo",
        // Maquillaje
        "Maquillaje social", "Maquillaje de novia"
    };

    private const string AdminEmail = "admin@salonunas.com";
    private const string AdminPassword = "Admin@Elara2026";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "Administrador Elara",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }

        foreach (var nombre in EspecialidadesBase)
        {
            var existe = await context.Especialidades.AnyAsync(e => e.Nombre == nombre);
            if (!existe)
            {
                context.Especialidades.Add(new Especialidad { Nombre = nombre });
            }
        }

        await context.SaveChangesAsync();
    }
}
