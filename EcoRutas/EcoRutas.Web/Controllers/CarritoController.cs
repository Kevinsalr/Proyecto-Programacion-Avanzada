using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Web.Models;

namespace EcoRutas.Web.Controllers
{
    // El carrito vive en la sesión y no aparta cupos: los cupos se descuentan al confirmar la reserva.
    [Authorize(Roles = "Asociado")]
    public class CarritoController : BaseController
    {
        private const int MaxPasajeros = 6;

        // GET: Carrito
        public ActionResult Index()
        {
            return View(ArmarCarrito());
        }

        // POST: Carrito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(int salidaId, int cantidad)
        {
            var salida = Servicios.Catalogo.ObtenerSalidasParaCarrito(new[] { salidaId }).FirstOrDefault();
            if (salida == null || !salida.Disponible)
            {
                MostrarError("La salida seleccionada ya no está disponible.");
                return RedirectToAction("Index", "Catalogo");
            }

            var error = ValidarCantidad(cantidad, salida.CuposDisponibles);
            if (error != null)
            {
                MostrarError(error);
                return RedirectToAction("Detalle", "Catalogo", new { id = salida.TourId });
            }

            var carrito = CarritoSesion.Obtener(Session);
            var item = carrito.FirstOrDefault(i => i.SalidaId == salidaId);
            if (item == null)
                carrito.Add(new CarritoItem { SalidaId = salidaId, CantidadPasajeros = cantidad });
            else
                item.CantidadPasajeros = cantidad;

            MostrarExito("Salida agregada al carrito. Los cupos no se apartan hasta que confirme la reserva.");
            return RedirectToAction("Index");
        }

        // POST: Carrito/Actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Actualizar(int salidaId, int cantidad)
        {
            var item = CarritoSesion.Obtener(Session).FirstOrDefault(i => i.SalidaId == salidaId);
            if (item != null)
            {
                if (cantidad < 1 || cantidad > MaxPasajeros)
                    MostrarError("La cantidad debe estar entre 1 y " + MaxPasajeros + " pasajeros.");
                else
                    item.CantidadPasajeros = cantidad;
            }
            return RedirectToAction("Index");
        }

        // POST: Carrito/Quitar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Quitar(int salidaId)
        {
            CarritoSesion.Obtener(Session).RemoveAll(i => i.SalidaId == salidaId);
            return RedirectToAction("Index");
        }

        // POST: Carrito/Vaciar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vaciar()
        {
            CarritoSesion.Vaciar(Session);
            return RedirectToAction("Index");
        }

        // GET: Carrito/Confirmar  (pide nombre e identificación de cada pasajero)
        public ActionResult Confirmar()
        {
            var carrito = ArmarCarrito();
            if (!carrito.PuedeConfirmar)
            {
                MostrarError(carrito.Lineas.Any()
                    ? "Revise los avisos del carrito antes de confirmar."
                    : "El carrito está vacío.");
                return RedirectToAction("Index");
            }

            var modelo = new ConfirmarReservaViewModel
            {
                Lineas = carrito.Lineas.Select(l => new ConfirmarLineaViewModel
                {
                    SalidaId = l.SalidaId,
                    TourNombre = l.TourNombre,
                    FechaHora = l.FechaHora,
                    PrecioXPersona = l.PrecioXPersona,
                    Pasajeros = Enumerable.Range(0, l.CantidadPasajeros).Select(n => new PasajeroViewModel()).ToList()
                }).ToList()
            };
            return View(modelo);
        }

        // POST: Carrito/Confirmar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirmar(ConfirmarReservaViewModel modelo)
        {
            if (modelo == null || modelo.Lineas == null || !modelo.Lineas.Any())
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                try
                {
                    var lineas = modelo.Lineas.Select(l => new ReservaLineaDto
                    {
                        SalidaId = l.SalidaId,
                        Pasajeros = (l.Pasajeros ?? new List<PasajeroViewModel>())
                            .Select(p => new PasajeroDto { Nombre = p.Nombre, Identificacion = p.Identificacion })
                            .ToList()
                    }).ToList();

                    var reservaId = Servicios.Reservas.Reservar(User.Identity.GetUserId(), lineas);
                    CarritoSesion.Vaciar(Session);
                    MostrarExito("¡Reserva confirmada! Estos son los pases de cada pasajero.");
                    return RedirectToAction("Detalle", "Reservas", new { id = reservaId });
                }
                catch (NegocioException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            CompletarDatos(modelo);
            return View(modelo);
        }

        // ---------- Auxiliares ----------

        // Junta lo que hay en sesión con los datos actuales de cada salida (precio, cupos, estado).
        private CarritoViewModel ArmarCarrito()
        {
            var carrito = CarritoSesion.Obtener(Session);
            var salidas = Servicios.Catalogo.ObtenerSalidasParaCarrito(carrito.Select(i => i.SalidaId))
                .ToDictionary(s => s.SalidaId);

            var modelo = new CarritoViewModel();
            foreach (var item in carrito)
            {
                var linea = new CarritoLineaViewModel
                {
                    SalidaId = item.SalidaId,
                    CantidadPasajeros = item.CantidadPasajeros
                };

                SalidaCarritoDto salida;
                if (!salidas.TryGetValue(item.SalidaId, out salida))
                {
                    linea.TourNombre = "Salida eliminada";
                    linea.Aviso = "Esta salida ya no existe. Quítela del carrito.";
                }
                else
                {
                    linea.TourId = salida.TourId;
                    linea.TourNombre = salida.TourNombre;
                    linea.FechaHora = salida.FechaHora;
                    linea.PrecioXPersona = salida.PrecioXPersona;
                    linea.CuposDisponibles = salida.CuposDisponibles;

                    if (!salida.Disponible)
                        linea.Aviso = "Esta salida ya no está disponible. Quítela del carrito.";
                    else if (salida.CuposDisponibles < item.CantidadPasajeros)
                        linea.Aviso = "Solo quedan " + salida.CuposDisponibles + " cupos. Reduzca la cantidad de pasajeros.";
                }

                modelo.Lineas.Add(linea);
            }
            return modelo;
        }

        // Vuelve a cargar los datos que solo se muestran (no se confía en lo que llega del formulario).
        private void CompletarDatos(ConfirmarReservaViewModel modelo)
        {
            var salidas = Servicios.Catalogo.ObtenerSalidasParaCarrito(modelo.Lineas.Select(l => l.SalidaId))
                .ToDictionary(s => s.SalidaId);

            foreach (var linea in modelo.Lineas)
            {
                if (linea.Pasajeros == null) linea.Pasajeros = new List<PasajeroViewModel>();

                SalidaCarritoDto salida;
                if (salidas.TryGetValue(linea.SalidaId, out salida))
                {
                    linea.TourNombre = salida.TourNombre;
                    linea.FechaHora = salida.FechaHora;
                    linea.PrecioXPersona = salida.PrecioXPersona;
                }
            }
        }

        private static string ValidarCantidad(int cantidad, int cuposDisponibles)
        {
            if (cantidad < 1 || cantidad > MaxPasajeros)
                return "La cantidad debe estar entre 1 y " + MaxPasajeros + " pasajeros.";
            if (cantidad > cuposDisponibles)
                return "Solo quedan " + cuposDisponibles + " cupos para esa salida.";
            return null;
        }
    }
}
