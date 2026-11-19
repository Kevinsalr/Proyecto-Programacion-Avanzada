using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Web.Models;

namespace EcoRutas.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : BaseController
    {
        // GET: Usuarios
        public ActionResult Index(string busqueda, string rol, string estado)
        {
            bool? activo = null;
            if (estado == "activos") activo = true;
            else if (estado == "inactivos") activo = false;

            var modelo = new UsuarioIndexViewModel
            {
                Busqueda = busqueda,
                Rol = rol,
                Estado = estado,
                Usuarios = Servicios.Usuarios.Listar(busqueda, rol, activo)
            };
            return View(modelo);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            return View(new UsuarioCrearViewModel());
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioCrearViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                Servicios.Usuarios.Crear(ADto(modelo), modelo.Password);
                MostrarExito("Usuario creado correctamente.");
                return RedirectToAction("Index");
            }
            catch (NegocioException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(modelo);
            }
        }

        // GET: Usuarios/Edit/id
        public ActionResult Edit(string id)
        {
            try
            {
                var dto = Servicios.Usuarios.ObtenerParaEditar(id);
                return View(new UsuarioFormViewModel
                {
                    Id = dto.Id,
                    CodigoUsuario = dto.CodigoUsuario,
                    NombreCompleto = dto.NombreCompleto,
                    Email = dto.Email,
                    Rol = dto.Rol,
                    Activo = dto.Activo
                });
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        // POST: Usuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UsuarioFormViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                Servicios.Usuarios.Editar(ADto(modelo), User.Identity.GetUserId());
                MostrarExito("Usuario actualizado correctamente.");
                return RedirectToAction("Index");
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
            catch (NegocioException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(modelo);
            }
        }

        // POST: Usuarios/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(string id, bool activo)
        {
            try
            {
                Servicios.Usuarios.CambiarEstado(id, activo, User.Identity.GetUserId());
                MostrarExito(activo ? "Usuario activado." : "Usuario inactivado.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Index");
        }

        // POST: Usuarios/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(string id)
        {
            try
            {
                Servicios.Usuarios.Eliminar(id, User.Identity.GetUserId());
                MostrarExito("Usuario eliminado.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Index");
        }

        // ---------- Auxiliares ----------

        private static UsuarioGuardarDto ADto(UsuarioFormViewModel m)
        {
            return new UsuarioGuardarDto
            {
                Id = m.Id,
                NombreCompleto = m.NombreCompleto,
                Email = m.Email,
                Rol = m.Rol,
                Activo = m.Activo
            };
        }
    }
}
