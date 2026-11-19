namespace EcoRutas.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using EcoRutas.Data.Context;
    using EcoRutas.Data.Entities;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<EcoRutasContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EcoRutasContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            foreach (var rol in new[] { "Administrador", "Asociado", "Guia" })
            {
                if (!roleManager.RoleExists(rol))
                    roleManager.Create(new IdentityRole(rol));
            }

            CrearUsuario(userManager, "USR-ADMIN01", "Administrador EcoRutas", "admin@ecorutas.com", "Admin123!", "Administrador");
            CrearUsuario(userManager, "USR-GUIA01", "Carlos Guía Rojas", "guia1@ecorutas.com", "Guia123!", "Guia");
            CrearUsuario(userManager, "USR-GUIA02", "María Guía Vargas", "guia2@ecorutas.com", "Guia123!", "Guia");
            CrearUsuario(userManager, "USR-ASOC01", "Ana Asociada Mora", "asociado@ecorutas.com", "Asociado123!", "Asociado");
        }

        private static void CrearUsuario(UserManager<ApplicationUser> manager, string codigo,
            string nombre, string email, string password, string rol)
        {
            if (manager.FindByEmail(email) != null) return;

            var usuario = new ApplicationUser
            {
                CodigoUsuario = codigo,
                NombreCompleto = nombre,
                UserName = email,
                Email = email,
                Activo = true
            };

            var resultado = manager.Create(usuario, password);
            if (resultado.Succeeded)
                manager.AddToRole(usuario.Id, rol);
        }
    }
}