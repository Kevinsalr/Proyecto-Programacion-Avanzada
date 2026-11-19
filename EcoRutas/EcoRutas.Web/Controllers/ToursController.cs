using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Web.Models;

namespace EcoRutas.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ToursController : BaseController
    {
        // GET: Tours
        public ActionResult Index(string busqueda, string estado)
        {
            bool? activo = null;
            if (estado == "activos") activo = true;
            else if (estado == "inactivos") activo = false;

            var modelo = new TourIndexViewModel
            {
                Busqueda = busqueda,
                Estado = estado,
                Tours = Servicios.Tours.Listar(busqueda, activo)
            };
            return View(modelo);
        }

        // GET: Tours/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                return View(Servicios.Tours.ObtenerDetalle(id));
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        // GET: Tours/Create
        public ActionResult Create()
        {
            return View(new TourFormViewModel());
        }

        // POST: Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TourFormViewModel modelo, IEnumerable<HttpPostedFileBase> imagenes)
        {
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                var id = Servicios.Tours.Crear(ADto(modelo), LeerImagenes(imagenes));
                MostrarExito("Tour creado correctamente.");
                return RedirectToAction("Details", new { id });
            }
            catch (NegocioException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(modelo);
            }
        }

        // GET: Tours/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var dto = Servicios.Tours.ObtenerParaEditar(id);
                return View(new TourFormViewModel
                {
                    Id = dto.Id,
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Categoria = dto.Categoria,
                    Dificultad = dto.Dificultad,
                    DuracionMinutos = dto.DuracionMinutos,
                    PuntoEncuentro = dto.PuntoEncuentro,
                    Activo = dto.Activo
                });
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        // POST: Tours/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TourFormViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                Servicios.Tours.Editar(ADto(modelo));
                MostrarExito("Tour actualizado correctamente.");
                return RedirectToAction("Details", new { id = modelo.Id });
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

        // POST: Tours/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, bool activo)
        {
            try
            {
                Servicios.Tours.CambiarEstado(id, activo);
                MostrarExito(activo ? "Tour activado." : "Tour inactivado.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Index");
        }

        // POST: Tours/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            try
            {
                Servicios.Tours.Eliminar(id);
                MostrarExito("Tour eliminado.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Index");
        }

        // POST: Tours/AgregarImagenes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarImagenes(int id, IEnumerable<HttpPostedFileBase> imagenes)
        {
            try
            {
                Servicios.Tours.AgregarImagenes(id, LeerImagenes(imagenes));
                MostrarExito("Imágenes agregadas.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Details", new { id });
        }

        // POST: Tours/EliminarImagen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarImagen(int id, int tourId)
        {
            try
            {
                Servicios.Tours.EliminarImagen(id);
                MostrarExito("Imagen eliminada.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Details", new { id = tourId });
        }

        // GET: Tours/Imagen/5  (sirve la imagen guardada en la base de datos)
        [AllowAnonymous]
        public ActionResult Imagen(int id)
        {
            try
            {
                var imagen = Servicios.Tours.ObtenerImagen(id);
                return File(imagen.Contenido, imagen.TipoContenido);
            }
            catch (NoEncontradoException)
            {
                return HttpNotFound();
            }
        }

        // ---------- Auxiliares ----------

        private static TourGuardarDto ADto(TourFormViewModel m)
        {
            return new TourGuardarDto
            {
                Id = m.Id,
                Codigo = m.Codigo,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Categoria = m.Categoria,
                Dificultad = m.Dificultad,
                DuracionMinutos = m.DuracionMinutos,
                PuntoEncuentro = m.PuntoEncuentro,
                Activo = m.Activo
            };
        }

        private static List<ImagenNuevaDto> LeerImagenes(IEnumerable<HttpPostedFileBase> archivos)
        {
            var lista = new List<ImagenNuevaDto>();
            if (archivos == null) return lista;

            foreach (var archivo in archivos)
            {
                if (archivo == null || archivo.ContentLength == 0) continue;

                using (var memoria = new MemoryStream())
                {
                    archivo.InputStream.CopyTo(memoria);
                    lista.Add(new ImagenNuevaDto
                    {
                        NombreArchivo = Path.GetFileName(archivo.FileName),
                        TipoContenido = archivo.ContentType,
                        Contenido = memoria.ToArray()
                    });
                }
            }
            return lista;
        }
    }
}