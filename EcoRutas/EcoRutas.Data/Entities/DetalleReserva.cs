using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class DetalleReserva
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }

        public int SalidaId { get; set; }

        [Range(1, 6)]
        [Display(Name = "Cantidad de pasajeros")]
        public int CantidadPasajeros { get; set; }

        [Display(Name = "Precio por persona")]
        [Range(typeof(decimal), "0.01", "9999999")]
        public decimal PrecioXPersona { get; set; }

        [Display(Name = "Estado")]
        public EstadoDetalle Estado { get; set; } = EstadoDetalle.Confirmada;

        [Display(Name = "Fecha de cancelación")]
        public DateTime? FechaCancelacion { get; set; }

        public virtual Reserva Reserva { get; set; }
        public virtual Salida Salida { get; set; }
        public virtual ICollection<Pase> Pases { get; set; } = new HashSet<Pase>();

        public decimal Subtotal => PrecioXPersona * CantidadPasajeros;
    }
}