using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Reserva
    {
        public Reserva()
        {
            Detalles = new HashSet<DetalleReserva>();
        }

        public int ReservaId { get; set; }

        [Required]
        public string AsociadoId { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual ApplicationUser Asociado { get; set; }

        public virtual ICollection<DetalleReserva> Detalles { get; set; }
    }
}
