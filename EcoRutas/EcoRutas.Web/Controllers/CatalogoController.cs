using System.Web.Mvc;
using EcoRutas.Service.Exceptions;
using EcoRutas.Web.Models;

namespace EcoRutas.Web.Controllers
{
    // Catálogo público: cualquier visitante puede ver los tours y sus próximas salidas.
    public class CatalogoController : BaseController
    {
        // GET: Catalogo
        public ActionResult Index(string busqueda, string categoria, string dificultad)
        {
            var modelo = new CatalogoIndexViewModel
            {
                Busqueda = busqueda,
                Categoria = categoria,
                Dificultad = dificultad,
                Tours = Servicios.Catalogo.ListarTours(busqueda, categoria, dificultad)
            };
            return View(modelo);
        }

        // GET: Catalogo/Detalle/5
        public ActionResult Detalle(int id)
        {
            try
            {
                return View(Servicios.Catalogo.ObtenerTour(id));
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}
