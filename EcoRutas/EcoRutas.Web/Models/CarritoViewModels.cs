using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EcoRutas.Web.Models
{
    // Línea del carrito guardada en sesión. No aparta cupos: solo recuerda la salida y la cantidad.
    [Serializable]
    public class CarritoItem
    {
        public int SalidaId { get; set; }
        public int CantidadPasajeros { get; set; }
    }

    // Lee y guarda el carrito en la sesión del usuario.
    public static class CarritoSesion
    {
        private const string Clave = "Carrito";

        public static List<CarritoItem> Obtener(HttpSessionStateBase sesion)
        {
            var carrito = sesion[Clave] as List<CarritoItem>;
            if (carrito == null)
            {
                carrito = new List<CarritoItem>();
                sesion[Clave] = carrito;
            }
            return carrito;
        }

        public static int Contar(HttpSessionStateBase sesion)
        {
            var carrito = sesion == null ? null : sesion[Clave] as List<CarritoItem>;
            return carrito == null ? 0 : carrito.Count;
        }

        public static void Vaciar(HttpSessionStateBase sesion)
        {
            sesion.Remove(Clave);
        }
    }

    // ---------- Pantalla del carrito ----------

    public class CarritoLineaViewModel
    {
        public int SalidaId { get; set; }
        public int TourId { get; set; }
        public string TourNombre { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal PrecioXPersona { get; set; }
        public int CantidadPasajeros { get; set; }
        public int CuposDisponibles { get; set; }
        public string Aviso { get; set; }   // por qué no se puede reservar esta línea (o null)

        public decimal Subtotal
        {
            get { return PrecioXPersona * CantidadPasajeros; }
        }
    }

    public class CarritoViewModel
    {
        public List<CarritoLineaViewModel> Lineas { get; set; } = new List<CarritoLineaViewModel>();

        public decimal Total
        {
            get { return Lineas.Sum(l => l.Subtotal); }
        }

        public bool PuedeConfirmar
        {
            get { return Lineas.Any() && Lineas.All(l => l.Aviso == null); }
        }
    }

    // ---------- Confirmar reserva: datos de cada pasajero ----------

    public class ConfirmarReservaViewModel
    {
        public List<ConfirmarLineaViewModel> Lineas { get; set; } = new List<ConfirmarLineaViewModel>();

        public decimal Total
        {
            get { return Lineas.Sum(l => l.PrecioXPersona * l.Pasajeros.Count); }
        }
    }

    public class ConfirmarLineaViewModel
    {
        public int SalidaId { get; set; }

        // Solo para mostrar; se vuelven a cargar desde el servicio al enviar el formulario.
        public string TourNombre { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal PrecioXPersona { get; set; }

        public List<PasajeroViewModel> Pasajeros { get; set; } = new List<PasajeroViewModel>();
    }

    public class PasajeroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar 120 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(30, ErrorMessage = "La identificación no puede superar 30 caracteres.")]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }
    }
}
