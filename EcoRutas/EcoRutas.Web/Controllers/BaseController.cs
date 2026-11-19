using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using EcoRutas.Data.Context;
using EcoRutas.Service.Implementaciones;

namespace EcoRutas.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private ServiceFactory _servicios;

        // Los controladores solo usan servicios; nunca consultan la base de datos.
        protected ServiceFactory Servicios
        {
            get
            {
                if (_servicios == null)
                {
                    var owin = HttpContext.GetOwinContext();
                    var contexto = owin.Get<EcoRutasContext>();
                    var userManager = owin.GetUserManager<ApplicationUserManager>();
                    _servicios = new ServiceFactory(contexto, userManager);
                }
                return _servicios;
            }
        }

        protected void MostrarExito(string mensaje)
        {
            TempData["Exito"] = mensaje;
        }

        protected void MostrarError(string mensaje)
        {
            TempData["Error"] = mensaje;
        }
    }
}