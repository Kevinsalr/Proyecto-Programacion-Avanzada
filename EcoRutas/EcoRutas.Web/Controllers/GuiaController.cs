using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EcoRutas.Service.Exceptions;

namespace EcoRutas.Web.Controllers
{
    // Vista del guía: solo consulta sus salidas y la lista de pasajeros.
    [Authorize(Roles = "Guia")]
    public class GuiaController : BaseController
    {
        // GET: Guia
        public ActionResult Index()
        {
            return View(Servicios.Guia.ListarSalidas(User.Identity.GetUserId()));
        }

        // GET: Guia/Pasajeros/5
        public ActionResult Pasajeros(int id)
        {
            try
            {
                return View(Servicios.Guia.ObtenerPasajeros(id, User.Identity.GetUserId()));
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}
