using System;
using System.Linq;
using System.Web.Mvc;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Web.Models;

namespace EcoRutas.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SalidasController : BaseController
    {
        // GET: Salidas/Create?tourId=5
        public ActionResult Create(int tourId)
        {
            try
            {
                var modelo = new SalidaFormViewModel
                {
                    TourId = tourId,
                    FechaHora = DateTime.Today.AddDays(7).AddHours(8),
                    CupoMax = 10,
                    Activa = true
                };
                Preparar(modelo);
                return View(modelo);
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index", "Tours");
            }
        }

        // POST: Salidas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SalidaFormViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Servicios.Salidas.Crear(ADto(modelo));
                    MostrarExito("Salida creada correctamente.");
                    return RedirectToAction("Details", "Tours", new { id = modelo.TourId });
                }
                catch (NoEncontradoException ex)
                {
                    MostrarError(ex.Message);
                    return RedirectToAction("Index", "Tours");
                }
                catch (NegocioException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            Preparar(modelo);
            return View(modelo);
        }

        // GET: Salidas/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var dto = Servicios.Salidas.ObtenerParaEditar(id);
                var modelo = new SalidaFormViewModel
                {
                    Id = dto.Id,
                    TourId = dto.TourId,
                    FechaHora = dto.FechaHora,
                    GuiaId = dto.GuiaId,
                    PrecioXPersona = dto.PrecioXPersona,
                    CupoMax = dto.CupoMax,
                    Activa = dto.Activa,
                    VersionFilaBase64 = dto.VersionFila == null ? null : Convert.ToBase64String(dto.VersionFila)
                };
                Preparar(modelo);
                return View(modelo);
            }
            catch (NoEncontradoException ex)
            {
                MostrarError(ex.Message);
                return RedirectToAction("Index", "Tours");
            }
        }

        // POST: Salidas/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SalidaFormViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Servicios.Salidas.Editar(ADto(modelo));
                    MostrarExito("Salida actualizada correctamente.");
                    return RedirectToAction("Details", "Tours", new { id = modelo.TourId });
                }
                catch (NoEncontradoException ex)
                {
                    MostrarError(ex.Message);
                    return RedirectToAction("Index", "Tours");
                }
                catch (NegocioException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            Preparar(modelo);
            return View(modelo);
        }

        // POST: Salidas/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, int tourId)
        {
            try
            {
                Servicios.Salidas.Eliminar(id);
                MostrarExito("Salida eliminada.");
            }
            catch (NegocioException ex)
            {
                MostrarError(ex.Message);
            }
            return RedirectToAction("Details", "Tours", new { id = tourId });
        }

        // ---------- Auxiliares ----------

        // Carga el nombre del tour y la lista de guías para el formulario.
        private void Preparar(SalidaFormViewModel modelo)
        {
            modelo.TourNombre = Servicios.Tours.ObtenerParaEditar(modelo.TourId).Nombre;
            modelo.Guias = Servicios.Salidas.ListarGuias()
                .Select(g => new SelectListItem
                {
                    Value = g.Id,
                    Text = g.NombreCompleto,
                    Selected = g.Id == modelo.GuiaId
                }).ToList();
        }

        private static SalidaGuardarDto ADto(SalidaFormViewModel m)
        {
            return new SalidaGuardarDto
            {
                Id = m.Id,
                TourId = m.TourId,
                FechaHora = m.FechaHora,
                GuiaId = m.GuiaId,
                PrecioXPersona = m.PrecioXPersona,
                CupoMax = m.CupoMax,
                Activa = m.Activa,
                VersionFila = string.IsNullOrEmpty(m.VersionFilaBase64)
                    ? null
                    : Convert.FromBase64String(m.VersionFilaBase64)
            };
        }
    }
}