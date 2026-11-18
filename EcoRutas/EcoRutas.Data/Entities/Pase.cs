using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRutas.Data.Entities
{
    public class Pase
    {
        public int PaseId { get; set; }

        [Required]
        public int DetalleReservaId { get; set; }

        [Required]
        [StringLength(40)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(120)]
        public string NombrePasajero { get; set; }

        [Required]
        [StringLength(30)]
        public string IdentificacionPasajero { get; set; }

        public bool Asistio { get; set; }

        public virtual DetalleReserva DetalleReserva { get; set; }
    }
}
