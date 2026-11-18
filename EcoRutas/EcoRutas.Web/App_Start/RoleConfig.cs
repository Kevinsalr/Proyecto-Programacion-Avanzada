using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using EcoRutas.Web.Models;
using EcoRutas.Data.Context;

namespace EcoRutas.Web.App_Start
{
    public class RoleConfig
    {
        public static void CrearRoles()
        {
            using (var context = new EcoRutasContext())
            {
                var roleManager = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context));

                string[] roles =
                {
                    "Usuario",
                    "Administrador"
                };

                foreach (string nombreRol in roles)
                {
                    if (!roleManager.RoleExists(nombreRol))
                    {
                        var resultado = roleManager.Create(
                            new IdentityRole(nombreRol));

                        if (!resultado.Succeeded)
                        {
                            throw new Exception(
                                "No se pudo crear el rol: " + nombreRol);
                        }
                    }
                }
            }
        }
    }
}