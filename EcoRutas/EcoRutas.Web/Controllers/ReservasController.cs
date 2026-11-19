using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EcoRutas.Service.Exceptions;

namespace EcoRutas.Web.Controllers
{
    // Historial del asociado: sus reservas, pases y cancelación.
    [Authorize(Roles = "Asociado")]
    public class ReservasController : BaseController
    {
        // GET: Reservas
        public ActionResult Index()
        {
            return View(Servicios.Reservas.ListarPorAsociado(User.Identity.GetUserId()));
        }

        // GET: Reservas/Detalle/5
        public ActionResult Detalle(int id)
        {
            try
            {
                return View(Servicios.Reservas.ObtenerDeAsociado(id, User.Identity.GetUserId()));
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        // POST: Reservas/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int detalleId)
        {
            try
            {
                Servicios.Reservas.CancelarDetalle(detalleId, User.Identity.GetUserId());
                MostrarExito("Reserva cancelada. Los cupos quedaron libres y sus pases fueron anulados.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
