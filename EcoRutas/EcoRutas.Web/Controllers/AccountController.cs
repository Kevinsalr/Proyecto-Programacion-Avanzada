using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EcoRutas.Web.Models;
using EcoRutas.Data.Entities;
using System;

namespace EcoRutas.Web.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ??
                    HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        // Mostrar formulario de registro.
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // Registrar un usuario nuevo.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    CodigoUsuario = "USR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    NombreCompleto = model.NombreCompleto,
                    UserName = model.Email,
                    Email = model.Email,
                    Activo = true
                };

                var result = await UserManager.CreateAsync(
                    user, model.Password);

                if (result.Succeeded)
                {
                    var roleResult = await UserManager.AddToRoleAsync(
                        user.Id, "Asociado");

                    if (!roleResult.Succeeded)
                    {
                        await UserManager.DeleteAsync(user);

                        AddErrors(roleResult);
                        return View(model);
                    }

                    await SignInAsync(user, false);

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            return View(model);
        }

        // Mostrar formulario de inicio de sesión.
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Iniciar sesión.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(
            LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await UserManager.FindAsync(
                model.Email, model.Password);

            if (user != null)
            {
                if (!user.Activo)
                {
                    ModelState.AddModelError(
                        "",
                        "Su cuenta está inactiva. Comuníquese con un administrador.");

                    return View(model);
                }

                user.UltimaConexion = DateTime.Now;
                await UserManager.UpdateAsync(user);

                await SignInAsync(user, model.RememberMe);

                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                "",
                "Correo electrónico o contraseña incorrectos.");

            return View(model);
        }

        // Cerrar sesión.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction("Index", "Home");
        }

        // Crear la sesión del usuario.
        private async Task SignInAsync(
            ApplicationUser user, bool rememberMe)
        {
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ExternalCookie);

            var identity = await UserManager.CreateIdentityAsync(
                user,
                DefaultAuthenticationTypes.ApplicationCookie);

            AuthenticationManager.SignIn(
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe
                },
                identity);
        }

        // Mostrar errores de Identity.
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}