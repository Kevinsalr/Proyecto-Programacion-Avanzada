using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcoRutas.Web.Controllers
{
    public class HomeController : BaseController
    {
        // Página de inicio: muestra hasta 3 tours con salidas disponibles.
        public ActionResult Index()
        {
            var destacados = Servicios.Catalogo.ListarTours(null, null, null)
                .Where(t => t.SalidasDisponibles > 0)
                .OrderBy(t => t.ProximaSalida)
                .Take(3)
                .ToList();
            return View(destacados);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}